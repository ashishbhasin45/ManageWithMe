using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddToDoHandler
{
    [DynamoDBTable("todos")]
    public class ToDo
    {
        public string Notes { get; set; }
        public string Title { get; set; }

        [DynamoDBHashKey]
        public string EmailId { get; set; }

        [DynamoDBRangeKey]
        public string TodoId { get; set; }

        public DateTime DueOn { get; set; }

        public Boolean IsCompleted { get; set; } = false;
        public Boolean HasFiles { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
