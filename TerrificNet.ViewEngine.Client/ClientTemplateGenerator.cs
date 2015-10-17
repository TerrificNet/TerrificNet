﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerrificNet.Thtml.Binding;
using TerrificNet.ViewEngine.Schema;
using Veil;
using Veil.Helper;
using Veil.Parser;
using Veil.Parser.Nodes;

namespace TerrificNet.ViewEngine.Client
{
	public class ClientTemplateGenerator : IClientTemplateGenerator
	{
	    private readonly IHelperHandlerFactory _helperHandlerFactory;
	    private readonly IMemberLocator _memberLocator;

	    public ClientTemplateGenerator(IHelperHandlerFactory helperHandlerFactory, IMemberLocator memberLocator)
	    {
		    _helperHandlerFactory = helperHandlerFactory;
		    _memberLocator = memberLocator;
	    }

	    public void Generate(TemplateInfo templateInfo, IClientContext clientContext, IClientModel clientModel)
	    {
		    using (var stream = new StreamReader(templateInfo.Open()))
		    {
			    GenerateForTemplate(templateInfo.Id, stream.ReadToEnd(), clientContext, clientModel);
		    }
	    }

		internal void GenerateForTemplate(string templateId, string template, IClientContext clientContext, IClientModel model)
	    {
			var parser = VeilStaticConfiguration.GetParserInstance("handlebars");
		    var helperHandlers = _helperHandlerFactory.Create().ToArray();

		    var tree = parser.Parse(templateId, new StringReader(template), typeof (object), _memberLocator, helperHandlers);
		    new ClientNodeVisitor(clientContext, model).Visit(tree);
	    }

		private class ClientNodeVisitor : NodeVisitorBase<IClientModel>
	    {
		    private readonly IClientContext _clientContext;
		    private readonly Stack<IClientModel> _modelStack = new Stack<IClientModel>(); 

		    public ClientNodeVisitor(IClientContext clientContext, IClientModel model)
		    {
			    _clientContext = clientContext;
				_modelStack.Push(model);
		    }

			protected override IClientModel VisitWriteLiteralNode(WriteLiteralNode writeLiteralNode)
		    {
			    _clientContext.WriteLiteral(writeLiteralNode.LiteralContent);
				return base.VisitWriteLiteralNode(writeLiteralNode);
			}

			protected override IClientModel VisitIterateNode(IterateNode iterateNode)
			{
				var result = this.VisitExpressionNode(iterateNode.Collection);
				
				_modelStack.Push(_clientContext.BeginIterate(result));
				this.Visit(iterateNode.Body);

				_modelStack.Pop();
				_clientContext.EndIterate();

				return result;
			}

			protected override IClientModel VisitWriteExpressionNode(WriteExpressionNode writeExpressionNode)
			{
				var result = this.VisitExpressionNode(writeExpressionNode.Expression);
                if (writeExpressionNode.HtmlEncode)
				    _clientContext.WriteEncodeExpression(result);
                else
                    _clientContext.WriteExpression(result);

				return result;
			}

		    protected override IClientModel VisitExpressionNode(ExpressionNode expression)
		    {
		        SelfExpressionNode node = expression as SelfExpressionNode;
		        if (node != null)
		            return VisitSelfExpressionNode(node);

		        return base.VisitExpressionNode(expression);
		    }

		    private IClientModel VisitSelfExpressionNode(SelfExpressionNode node)
		    {
		        return _modelStack.Peek();
		    }

		    protected override IClientModel VisitLateBoundExpression(LateBoundExpressionNode lateboundExpression)
			{
		        if (lateboundExpression.Scope == ExpressionScope.ModelOfParentScope)
		        {
		            var self = _modelStack.Pop();
		            var outer = _modelStack.Peek();
		            _modelStack.Push(self);
		            
                    return outer.Get(lateboundExpression.ItemName);
		        }

				return _modelStack.Peek().Get(lateboundExpression.ItemName);
			}

			protected override IClientModel VisitSubModelExpressionNode(SubModelExpressionNode subModuleExpression)
			{
				_modelStack.Push(this.VisitExpressionNode(subModuleExpression.ModelExpression));
				var result = this.VisitExpressionNode(subModuleExpression.SubModelExpression);

				_modelStack.Pop();
				return result;
			}

			protected override IClientModel VisitHelperNode(HelperExpressionNode helperNode)
			{
				var helperHandlerClient = helperNode.HelperHandler as IHelperHandlerClient;
				if (helperHandlerClient != null)
					return helperHandlerClient.Evaluate(_clientContext, _modelStack.Peek(), helperNode.Name, helperNode.Parameters);

				return base.VisitHelperNode(helperNode);
			}

			protected override IClientModel VisitHelperBlockNode(HelperBlockNode helperBlockNode)
			{
				var helperNode = helperBlockNode.HelperExpression;
				var helperHandlerClient = helperNode.HelperHandler as IBlockHelperHandlerClient;
				if (helperHandlerClient != null)
					_modelStack.Push(helperHandlerClient.Evaluate(_clientContext, _modelStack.Peek(), helperNode.Name, helperNode.Parameters));

				this.Visit(helperBlockNode.Block);

				if (helperHandlerClient != null)
				{
					_modelStack.Pop();
					helperHandlerClient.Leave(_clientContext, _modelStack.Peek(), helperNode.Name, helperNode.Parameters);
				}
				return null;
			}

			protected override IClientModel VisitConditionalNode(ConditionalNode node)
			{
				var result = this.VisitExpressionNode(node.Expression);

				_clientContext.BeginIf(result);
				this.VisitBlockNode(node.TrueBlock);
				if (node.FalseBlock != null)
				{
					_clientContext.ElseIf();
					this.VisitBlockNode(node.FalseBlock);
				}

				_clientContext.EndIf();
				return null;
			}
	    }
    }
}
