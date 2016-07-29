using System;
using System.Collections.Generic;

namespace Engine.Cloud.Core.Model
{
    public class InvoiceDetails
    {
        public Int64 SnapshotId { get; set; }
        public string InitialDate { get; set; }
        public string FinalDate { get; set; }
        public string IsPostPaid { get; set; }
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public List<Group> ServiceGroup { get; set; }

        public class Group
        {
            public Int64 Id { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
            public Int64 TypeId { get; set; }
            public List<Resource> Resources { get; set; }
            public Adjustment Adjustment { get; set; }
            public string Total { get; set; }
        }

        public class Resource
        {
            public Int64 Id { get; set; }
            public string Name { get; set; }
            public string Metric { get; set; }
            public List<Usage> Usages { get; set; }
            public decimal Total { get; set; }
        }

        public class Usage
        {
            public Int64 Id { get; set; }
            public string InitialPeriod { get; set; }
            public string FinalPeriod { get; set; }
            public decimal ChargeValue { get; set; }
            public decimal Value { get; set; }
            public Int64 TypeChargeId { get; set; }
        }

        public class Adjustment
        {
            public decimal Total { get; set; }
        }
    }
}
