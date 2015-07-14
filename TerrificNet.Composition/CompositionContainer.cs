using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Composition
{
    public class CompositionContainer
    {
        private readonly HashSet<IModule> _modules = new HashSet<IModule>();

        public void RegisterModule(IModule module)
        {
            if (_modules.Contains(module))
                throw new ArgumentException("The given module is already registred", "module");

            _modules.Add(module);
        }

        public IEnumerable<IModule> GetModules()
        {
            return _modules;
        }

        public IEnumerable<Requirement> GetRequirements()
        {
            return _modules.SelectMany(m => EmptyOnNull(m.GetRequirements()));
        }

        private static IEnumerable<T> EmptyOnNull<T>(IEnumerable<T> req)
        {
            return req ?? Enumerable.Empty<T>();
        }

        public IEnumerable<Offer> GetOffers()
        {
            return _modules.SelectMany(m => EmptyOnNull(m.GetOffers()));
        }

        public VerificationResult Verify()
        {
            var requirements = this.GetRequirements().ToList();
            var offers = this.GetOffers().ToList();

            var missing = new List<Requirement>();
            var notUnique = new List<NotUniqueConfiguration>();

            foreach (var req in requirements)
            {
                var matchingOffers = req.GetMatchingOffers(offers).ToList();
                if (matchingOffers.Count == 0)
                    missing.Add(req);

                if (matchingOffers.Count > 1)
                    notUnique.Add(new NotUniqueConfiguration(req, matchingOffers));
            }

            return new VerificationResult(missing, notUnique);
        }

        public class VerificationResult
        {
            private readonly List<NotUniqueConfiguration> _notUnique;
            private readonly IList<Requirement> _missingRequirements;

            public VerificationResult()
            {
                this.Success = true;
                _missingRequirements = new List<Requirement>();
            }

            public VerificationResult(IEnumerable<Requirement> missingRequirements, List<NotUniqueConfiguration> notUnique)
            {
                _notUnique = notUnique;
                _missingRequirements = missingRequirements.ToList();
                this.Success = _missingRequirements.Count == 0 && notUnique.Count == 0;
            }

            public bool Success { get; private set; }

            public IEnumerable<Requirement> GetMissingRequirements()
            {
                return _missingRequirements;
            }

            public IEnumerable<NotUniqueConfiguration> GetNotMatchingRequirements()
            {
                return _notUnique;
            }
        }

        public class NotUniqueConfiguration
        {
            public NotUniqueConfiguration(Requirement requirement, IReadOnlyList<Offer> offers)
            {
                this.Requirement = requirement;
                this.Offers = offers;
            }

            public Requirement Requirement { get; private set; }
            public IReadOnlyList<Offer> Offers { get; private set; }
        }

    }

}
