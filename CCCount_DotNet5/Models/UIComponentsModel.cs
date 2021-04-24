using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCCount.Models
{
    public class Brand
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public List<Division> Divisions { get; set; }
        public List<Market> Markets { get; set; }
        public List<Channel> Channels { get; set; }
        public List<Attribute> Attribute1 { get; set; }
        public List<Attribute> Attribute2 { get; set; }

        public bool IsTimeOnOfferEnabled { get; set; }
        public bool IsRankEnabled { get; set; }
        public bool IsSeasonCodeEnabled { get; set; }

        public Brand()
        {
            this.Divisions = new List<Division>();
            this.Markets = new List<Market>();
            this.Channels = new List<Channel>();
            this.Attribute1 = new List<Attribute>();
            this.Attribute2 = new List<Attribute>();
        }
    }

    public class Market
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }

        public Market() { }
    }

    public class Channel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }

        public Channel() { }
    }

    public class StartDateYear
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public StartDateYear() { }
    }

    public class StartDateMonth
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public StartDateMonth() { }
    }

    public class Division
    {
        public List<Department> Departments { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Division()
        {
            this.Departments = new List<Department>();
        }
    }

    public class Department
    {
        public List<Class> Classes { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Department()
        {
            this.Classes = new List<Class>();
        }
    }

    public class Class
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public List<SubClass> SubClasses { get; set; }
        public Class() { }
    }

    public class SubClass
    {
        #region Properties

        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The selected index.
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a default Sub Class.
        /// </summary>
        public SubClass() { }

        /// <summary>
        /// Constructs a Sub Class.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="Value">The value.</param>
        public SubClass(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        #endregion Constructors
    }
    public class Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Brand { get; set; }

        public Attribute() { }
    }

    public class ProgramCountProxy
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }

        public ProgramCountProxy() { }
    }
}
