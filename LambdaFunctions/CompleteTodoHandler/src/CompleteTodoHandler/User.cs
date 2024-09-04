using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteTodoHandler
{
    [DynamoDBTable("User")]
    public class User
    {
        [DynamoDBHashKey]
        public string Email { get; set; }

        [DynamoDBProperty]
        public int Otp { get; set; }
    }
}
