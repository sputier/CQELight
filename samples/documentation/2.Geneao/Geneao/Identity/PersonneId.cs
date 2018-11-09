using System;

namespace Geneao.Identity
{
    public sealed class PersonneId
    {

        public Guid Value { get; private set; }

        public PersonneId(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidOperationException("PersonneId.ctor() : A valid id should be provided.");
            Value = value;
        }

        public static PersonneId Generate()
            => new PersonneId(Guid.NewGuid());
    }
}
