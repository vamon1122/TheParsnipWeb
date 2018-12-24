﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollApi
{
    class PollOption
    {
        public Guid id { get; set; }
        public Guid pollId { get; set; }
        public Guid createdByUserId { get; set; }
        public DateTime dateCreated { get; set; }
        public string value { get; set; }

        public PollOption(Guid pPollId)
        {
            id = pPollId;
        }

        public PollOption()
        {
            id = Guid.NewGuid();
            dateCreated = DateTime.Now.AddHours(8);
        }

        public bool Insert()
        {
            return Data.InsertPollOption(this);

        }
    }
}
