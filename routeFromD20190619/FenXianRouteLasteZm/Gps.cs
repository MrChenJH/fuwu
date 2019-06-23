using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FenXianRouteLasteZm
{
    public class Gps
    {

        private double wgLat;
        private double wgLon;

        public Gps(double wgLat, double wgLon)
        {
            setWgLat(wgLat);
            setWgLon(wgLon);
        }

        public double getWgLat()
        {
            return wgLat;
        }

        public void setWgLat(double wgLat)
        {
            this.wgLat = wgLat;
        }

        public double getWgLon()
        {
            return wgLon;
        }

        public void setWgLon(double wgLon)
        {
            this.wgLon = wgLon;
        }

        override
        public string ToString()
        {
            return this.wgLat.ToString() + "," + this.wgLon.ToString();
        }

    }

}
