using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.MongoDb.Mapping
{
    public class IndexDetail
    {
        #region Properties

        public IEnumerable<string> Properties { get; set; }
        public bool Unique { get; set; }

        #endregion
    }
}
