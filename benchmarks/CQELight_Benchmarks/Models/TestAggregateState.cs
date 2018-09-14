using CQELight.Abstractions.DDD;

namespace CQELight_Benchmarks.Models
{

    public class TestAggregateState : AggregateState
    {

        #region Properties

        public int AggInt { get; private set; }
        public string AggString { get; private set; }
        public int InternalCount { get; private set; }

        #endregion

        #region Ctor

        internal TestAggregateState()
        {
            AddHandler<TestEvent>(AggregateEventHandler);
        }

        #endregion

        #region Public methods

        private void AggregateEventHandler(TestEvent evt)
        {
            AggInt = evt.AggregateIntValue;
            AggString = evt.AggregateStringValue;
            InternalCount++;
        }


        #endregion

    }

}
