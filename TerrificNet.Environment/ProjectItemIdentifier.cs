using System;

namespace TerrificNet.Environment
{
    public class ProjectItemIdentifier : IEquatable<ProjectItemIdentifier>
    {
        public bool Equals(ProjectItemIdentifier other)
        {
            return string.Equals(Identifier, other.Identifier) && Equals(Kind, other.Kind);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Identifier?.GetHashCode() ?? 0)*397) ^ (Kind?.GetHashCode() ?? 0);
            }
        }

        public ProjectItemIdentifier(string identifier, ProjectItemKind kind)
        {
            Identifier = identifier;
            Kind = kind;
        }

        public string Identifier { get; }
        public ProjectItemKind Kind { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectItemIdentifier) obj);
        }
    }
}