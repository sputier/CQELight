using System;

namespace Geneao.Identity
{
    public sealed class FamilleId
    {

        public Guid Value { get; private set; }

        public FamilleId(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidOperationException("FamilleId.ctor() : A valid id should be provided.");
            Value = value;
        }

        public static FamilleId Generate()
            => new FamilleId(Guid.NewGuid());
    }
}
