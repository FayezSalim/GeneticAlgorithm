using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    public class genome
    {
       public List<int> order=new List<int>();
       public string orderstring {get; set;}
       public int distance{get; set;}
    

        public void makestring()
       {
           orderstring = "";
            for(int i=0;i<order.Count;i++)
            {
                orderstring += Convert.ToString(order[i]) + " ";
            }
        }

        //calc distance param dist 2d array
    }
}
