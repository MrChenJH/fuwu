using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FenXianRouteLasteZm
{
    public class Result<T>
    {

        public int status { get; set; }

        public string message { get; set; }

        public List<T> result { get; set; }
    }
    public class V
    {
        public distance distance { get; set; }
        public duration duration { get; set; }
    }
    public class distance
    {
        public string text { get; set; }
        public double value { get; set; }
    }
    public class Udistance
    {
        public string nbbh { get; set; }
        public string text { get; set; }
        public double value { get; set; }
    }
    public class duration
    {
        public string text { get; set; }
        public double value { get; set; }
    }

    public class V1
    {
        public float x { get; set; }
        public float y { get; set; }
    }


}
