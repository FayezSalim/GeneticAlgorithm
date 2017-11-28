using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace TSP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch watch = new Stopwatch();
        Int64 popsize = 10,generations=100;
        int sum = 0;
		BackgroundWorker GA = new BackgroundWorker();//with interface(list)
        BackgroundWorker GA1 = new BackgroundWorker();//with graph
        public ObservableCollection<city> cities = new ObservableCollection<city>();
        public ObservableCollection<genome> PresentPopData = new ObservableCollection<genome>();
        public ObservableCollection<genome> NewPopData = new ObservableCollection<genome>();
        public ObservableCollection<genome> BestGenome = new ObservableCollection<genome>();
        public List<genome> PresentPopData1 = new List<genome>();
        public List<genome> NewPopData1 = new List<genome>();
        public List<genome> BestGenome1 = new List<genome>();
        int[,] distances;
        double crossoverrate=0.6, mutationrate=0.3;
        Random crossoverrandom, mutaterandom,selecter;
        private object presentpoplock = new object();
        private object newpoplock = new object();
        private object bestgenomelock = new object();
        int pause = 100;

        public MainWindow()
        {
            InitializeComponent();
            crossoverrandom = new Random();
            mutaterandom = new Random();
            selecter = new Random();
            BindingOperations.EnableCollectionSynchronization(PresentPopData, presentpoplock);
            BindingOperations.EnableCollectionSynchronization(NewPopData, newpoplock);
            BindingOperations.EnableCollectionSynchronization(BestGenome, bestgenomelock);
            this.crossoverratetextbox.Text = Convert.ToString(crossoverrate);
            this.mutationratetextbox.Text = Convert.ToString(mutationrate);
            this.speed.Text = Convert.ToString(pause);
            this.gentextbox.Text = Convert.ToString(generations);
            GA.WorkerReportsProgress = true;
            GA.WorkerSupportsCancellation = true;
            GA1.WorkerReportsProgress = true;
            GA1.WorkerSupportsCancellation = true;
            GA1.ProgressChanged += GA1_ProgressChanged;
            GA1.DoWork += GA1_DoWork;
            GA.ProgressChanged+=GA_ProgressChanged;
        }

        public void GA1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                watch.Start();
                for (int gen = 0; gen < generations; gen++)//generations
                {
                    if (GA1.CancellationPending == true)
                    { return; }
                    etilism1();//etilism
                    for (int i = 2; i < popsize; i += 2)
                    {
                        genome parent1 = selection1();//select parent1
                        genome parent2 = selection1();//select parent2
                        genome[] children = crossover(parent1, parent2);//crossover
                        children[0] = mutate(children[0]);//mutate 1st child
                        children[1] = mutate(children[1]);//mutate 2nd child
                        storenewpop1(children[0]);//store 1st child
                        storenewpop1(children[1]);//store 2nd child
                    }
                    setcurrpop1();
                    GA1.ReportProgress(gen, PresentPopData1);
                    Thread.Sleep(pause);
                }
                bestsoln1();
            }
            catch(Exception f)
            {
                MessageBox.Show(f.Message,"Something went wrong", MessageBoxButton.OK);
            }
        }

        public void GA1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.currgen.Content = e.ProgressPercentage + 1;
            TimeSpan ts = watch.Elapsed;
            this.elaps.Content=ts.Days+ " days "+ts.Hours+":"+ts.Minutes+ ":"+ts.Seconds+":"+ts.Milliseconds;
            List<genome> k=(List<genome>)e.UserState;
            Double[] arr=new Double[popsize];
            Double temp;
            int best = k[0].distance;
            for(int i=0;i<k.Count;i++)
            {
                arr[i]=k[i].distance;
                if(arr[i]<best)
                {
                    best =(int) arr[i];
                }
            }
            if(Convert.ToInt32(this.currbest.Content)!=best)
            {
            this.currbest.Content = best;
            this.bestgen.Content = e.ProgressPercentage + 1 ;
            this.besttime.Content = System.DateTime.Now.ToString("dd/MM/yy hh:mm:ss tt");
            }
            if (e.ProgressPercentage + 1 == 1)
            {
                this.strt.Content = System.DateTime.Now.ToString("dd/MM/yy hh:mm:ss tt");
            }
            else if (e.ProgressPercentage + 1 == generations)
            {
                this.end.Content = System.DateTime.Now.ToString("dd/MM/yy hh:mm:ss tt");
                watch.Stop();
                this.dist.Content = this.currbest.Content;
                for (int i = 0; i < k.Count; i++)
                {
                    if (k[i].distance == Convert.ToInt32(this.currbest.Content))
                    {
                        this.order.Content = k[i].orderstring;
                        break;
                    }
                }
            }
            for (int i = 0; i < k.Count; i++)
            {
                for (int j = i + 1; j < k.Count; j++)
                {
                    if (arr[i] > arr[j])
                    {
                        temp=arr[i];
                        arr[i] = arr[j];
                        arr[j] = temp;
                    }
                }
            }
            Double min = arr[0];
            for (int i = 0; i < k.Count;i++ )
            {
               arr[i] = arr[i] / min;
            }
            rect1.Height = rect11.Height;
            rect1.Fill = brush(rect1.Height);
            rect1.UpdateLayout();
            rect11.Height = 360 / arr[0];
            rect11.Fill = brush(rect11.Height);
            rect11.UpdateLayout();
            rect2.Height = rect12.Height;
            rect2.Fill = brush(rect2.Height);
            rect2.UpdateLayout();
            rect12.Height = 360 / arr[1];
            rect12.Fill = brush(rect12.Height);
            rect12.UpdateLayout();
            rect3.Height = rect13.Height;
            rect3.Fill = brush(rect3.Height);
            rect3.UpdateLayout();
            rect13.Height = 360 / arr[2];
            rect13.Fill = brush(rect13.Height);
            rect13.UpdateLayout();
            rect4.Height = rect14.Height;
            rect4.Fill = brush(rect4.Height);
            rect4.UpdateLayout();
            rect14.Height = 360 / arr[3];
            rect14.Fill = brush(rect14.Height);
            rect14.UpdateLayout();
            rect5.Height = rect15.Height;
            rect5.Fill = brush(rect5.Height);
            rect5.UpdateLayout();
            rect15.Height = 360 / arr[4];
            rect15.Fill = brush(rect15.Height);
            rect15.UpdateLayout();
            rect6.Height = rect16.Height;
            rect6.Fill = brush(rect6.Height);
            rect6.UpdateLayout();
            rect16.Height = 360 / arr[5];
            rect16.Fill = brush(rect16.Height);
            rect16.UpdateLayout();
            rect7.Height = rect17.Height;
            rect7.Fill = brush(rect7.Height);
            rect7.UpdateLayout();
            rect17.Height = 360 / arr[6];
            rect17.Fill = brush(rect17.Height);
            rect17.UpdateLayout();
            rect8.Height = rect18.Height;
            rect8.Fill = brush(rect8.Height);
            rect8.UpdateLayout();
            rect18.Height = 360 / arr[7];
            rect18.Fill = brush(rect18.Height);
            rect18.UpdateLayout();
            rect9.Height = rect19.Height;
            rect9.Fill = brush(rect9.Height);
            rect9.UpdateLayout();
            rect19.Height = 360 / arr[8];
            rect19.Fill = brush(rect19.Height);
            rect19.UpdateLayout();
            rect10.Height = rect20.Height;
            rect10.Fill = brush(rect10.Height);
            rect10.UpdateLayout();
            rect20.Height = 360 / arr[9];
            rect20.Fill = brush(rect20.Height);
            rect20.UpdateLayout();        
        }

        private LinearGradientBrush brush(double y)
        {
            LinearGradientBrush myBrush;
            if (y < 100)
            {
                myBrush = new LinearGradientBrush(Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 255, 0), new Point(0, 0), new Point(0, 0));
            }
            else if (y < 170)
            {
                myBrush = new LinearGradientBrush(Colors.Yellow, Color.FromArgb(255, 0, 255, 0), new Point(0, 0), new Point(0, 0.9));
            }
            else if (y < 230)
            {
                myBrush = new LinearGradientBrush(Colors.Yellow, Color.FromArgb(255, 0, 255, 0), new Point(0, 0.4), new Point(0, 1));
            }
            else if (y < 280)
            {
                myBrush = new LinearGradientBrush(Colors.Orange, Color.FromArgb(255, 0, 255, 0), new Point(0, 0.1), new Point(0, 1));
                myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.5));
            }
            else
            {
                myBrush = new LinearGradientBrush(Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 255, 0), new Point(0, 0), new Point(0, 1));
                myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.5));
            }
            return myBrush;
        }

        public ObservableCollection<city> CityCollection
        {
            get { return cities; }
        }

        public ObservableCollection<genome> PresentPopCollection
        {
            get { return PresentPopData; }
        }
       
        public ObservableCollection<genome> NewPopCollection
        {
            get { return NewPopData; }
        }
        
        public ObservableCollection<genome> BestGenomeCollection
        {
            get { return BestGenome; }
        }

        private void addcity_Click(object sender, RoutedEventArgs e)
        {
            cities.Add(new city { no = Convert.ToString(this.citylistdisplay.Items.Count)});
        }

        private void generaterandomdist()
        {
            distances = new int[this.citylistdisplay.Items.Count, this.citylistdisplay.Items.Count];
            Random r = new Random();
            for (int i = 0; i < this.citylistdisplay.Items.Count; i++)
            {
                distances[i, i] = 0;
                for(int j=i+1;j<this.citylistdisplay.Items.Count;j++)
                {
                    distances[i, j] = distances[j, i] = r.Next(1, 10);
                }
            }
            for(int i=0;i<this.cities.Count;i++)
            {
                cities[i].dist = "";
                for(int j=0;j<this.cities.Count;j++)
                {
                    cities[i].dist += Convert.ToString(distances[i, j]) + ",";
                }
            }
            this.citylistdisplay.Items.Refresh();
        }

        private void gendist_Click(object sender, RoutedEventArgs e)
        {
            generaterandomdist();
        }

        private void delcity_Click(object sender, RoutedEventArgs e)
        {
            cities.Clear();
            PresentPopData.Clear();
            NewPopData.Clear();
            BestGenome.Clear();
            PresentPopData1.Clear();
            NewPopData1.Clear();
            BestGenome1.Clear();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if(this.cities.Count<3)
            {
                MessageBox.Show("Please add atleast 3 cities", "Alert",MessageBoxButton.OK);
                return;
            }
            if ((int)Convert.ToUInt32(this.speed.Text)<10)
            {
                this.speed.Text = "10";
            }
            pause =(int) Convert.ToUInt64(this.speed.Text);
            generations = (Int64)Convert.ToUInt64(this.gentextbox.Text);
            crossoverrate = Convert.ToDouble(this.crossoverratetextbox.Text);
            mutationrate = Convert.ToDouble(this.mutationratetextbox.Text);
            sum = 0;
            for (int i = 1; i <= popsize; i++)//sum
            {
                sum += i;
            }
            MessageBoxResult res= MessageBox.Show("Do you want visualization of the process", "Alert", MessageBoxButton.YesNo);
            if(res==MessageBoxResult.Yes)
            {
                //start background wrker
                if ((int)Convert.ToUInt32(this.speed.Text) < 20)
                {
                    this.speed.Text = "20";
                }
                pause = (int)Convert.ToUInt32(this.speed.Text);
                PresentPopData.Clear();
                NewPopData.Clear();
                BestGenome.Clear();
                startpop();
                bestsoln();
                MessageBox.Show("Running Visualization for very large input sets may result in an application crash,If you want the results for such a set then please avoid using visualization ", "WARNING", MessageBoxButton.OK,MessageBoxImage.Warning);
                GA.DoWork += GA_DoWork;
                GA.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("The application will stop responding for a while please be patient", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                Storyboard n = (Storyboard)TryFindResource("conver");
                n.Begin();
                PresentPopData1.Clear();
                NewPopData1.Clear();
                BestGenome1.Clear();
                startpop1();
                this.strt.Content = "";
                this.end.Content = "";
                this.currbest.Content = 0;
                this.bestgen.Content = "";
                this.besttime.Content = "";
                this.elaps.Content = "";
                watch.Reset();
                GA1.RunWorkerAsync();
            }
        }

        public void GA_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.genlabel.Content = e.ProgressPercentage+1;
        }

        void GA_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (int gen = 0; gen < generations; gen++)//generations
                {
                    if (GA.CancellationPending)
                    {
                        break;
                    }
                    while (NewPopData.Count > 0)
                    {
                        NewPopData.Clear();
                    }
                    etilism();//etilism
                    for (int i = 2; i < popsize; i += 2)
                    {
                        genome parent1 = selection();//select parent1
                        genome parent2 = selection();//select parent2
                        genome[] children = crossover(parent1, parent2);//crossover
                        children[0] = mutate(children[0]);//mutate 1st child
                        children[1] = mutate(children[1]);//mutate 2nd child
                        storenewpop(children[0]);//store 1st child
                        storenewpop(children[1]);//store 2nd child
                        Thread.Sleep(pause);
                    }
                    while(PresentPopData.Count>0)
                    {
                        PresentPopData.Clear();
                    }
                    setcurrpop();
                    while(NewPopData.Count>0)
                    {
                        NewPopData.Clear();
                    }
                    bestsoln();
                    this.GA.ReportProgress(gen);
                    Thread.Sleep(pause);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please run this input set without visualization"+" "+ex.Message, "Alert", MessageBoxButton.OK);
            }
        }

        //select random start population
        private void startpop()
        {
            PresentPopData.Clear();
            NewPopData.Clear();
            genome g = new genome();
            for(int i=0;i<this.cities.Count;i++)
            {
                g.order.Add(i);
            }
            g.distance = genomefitness(g);
            g.makestring();
            PresentPopData.Add(g);
            g = null;
            g = new genome();
            g.distance = 0;
            if(this.cities.Count%2==0)
            {
                for (int i = this.cities.Count/2; i < this.cities.Count; i++)
                {
                    g.order.Add(i);
                }
                for (int i = (this.cities.Count / 2)-1; i >-1; i--)
                {
                    g.order.Add(i);
                }            }
            else
            {
                for (int i = this.cities.Count+1 / 2; i < this.cities.Count; i++)
                {
                    g.order.Add(i);
                }
                for (int i = (this.cities.Count+1 / 2) - 1; i > -1; i--)
                {
                    g.order.Add(i);
                }
            }
            g.makestring();
            g.distance = genomefitness(g);
            PresentPopData.Add(g);
            Random m = new Random();
            for(int i=2;i<popsize;i++)
            {
            g=new genome();
            int[] array = randomarray(m);
            for(int k=0;k<this.cities.Count;k++ )
            {
                g.order.Add(array[k]);
            }
            g.distance = genomefitness(g);
            g.makestring();
            PresentPopData.Add(g);
            }
        }

        //select random start population
        private void startpop1()
        {
            PresentPopData1.Clear();
            NewPopData1.Clear();
            genome g = new genome();
            for (int i = 0; i < this.cities.Count; i++)
            {
                g.order.Add(i);
            }
            g.distance = genomefitness(g);
            g.makestring();
            PresentPopData1.Add(g);
            g = null;
            g = new genome();
            g.distance = 0;
            if (this.cities.Count % 2 == 0)
            {
                for (int i = this.cities.Count / 2; i < this.cities.Count; i++)
                {
                    g.order.Add(i);
                }
                for (int i = (this.cities.Count / 2) - 1; i > -1; i--)
                {
                    g.order.Add(i);
                }
            }
            else
            {
                for (int i = this.cities.Count + 1 / 2; i < this.cities.Count; i++)
                {
                    g.order.Add(i);
                }
                for (int i = (this.cities.Count + 1 / 2) - 1; i > -1; i--)
                {
                    g.order.Add(i);
                }
            }
            g.makestring();
            g.distance = genomefitness(g);
            PresentPopData1.Add(g);
            Random m = new Random();
            for (int i = 2; i < popsize; i++)
            {
                g = new genome();
                int[] array = randomarray(m);
                for (int k = 0; k < this.cities.Count; k++)
                {
                    g.order.Add(array[k]);
                }
                g.distance = genomefitness(g);
                g.makestring();
                PresentPopData1.Add(g);
            }
        }

        //randomising array
        private int[] randomarray(Random m)
        {
            int[] array=new int[this.cities.Count];
            for(int i=0;i<this.cities.Count;i++)
            {
                array[i] = i;
            }
            for (int i = 0; i < 3; i++)
            {
                int l = m.Next(0, this.cities.Count - 1);
                int n = 0;
                do
                {
                    n = m.Next(0, this.cities.Count - 1);
                } while (n == l);
                int temp = array[l];
                array[l] = array[n];
                array[n] = temp;
            }
            return array;
        }

        //get total distance of a genome
        private int genomefitness(genome h)
        {
            int m=0;
            for (int i = 0; i < this.cities.Count; i++)
            {
                if(i+1==this.cities.Count)
                {
                    m += distances[h.order[i], h.order[0]];
                    break;
                }
                m+=distances[h.order[i],h.order[i+1]];
            }
            return m;
        }

        //selection
        private genome selection()//rank selection
        {
            genome selected;
            int[] distarray = new int[popsize];
            for (int i = 0; i < PresentPopData.Count; i++)
            {
                distarray[i] = PresentPopData[i].distance;
            }
            int temp;
            for (int i = 0; i < popsize; i++)
            {
                for (int j = 0; j < popsize; j++)
                {
                    if (distarray[i] < distarray[j])//descending 
                    {
                        temp = distarray[i];
                        distarray[i] = distarray[j];
                        distarray[j] = temp;
                    }
                }
            }
            int r = selecter.Next(0, sum);
            int accumulatefitness = 0;
            int select;
            for (select = 10; select > 0; select--)//fitness accumulation
            {
                accumulatefitness += select;
                if (accumulatefitness > r)
                {
                    break;
                }
            }
            for (int j = 0; j < popsize; j++)//find selected one in presentpop
            {
                if (distarray[select - 1] == PresentPopData[j].distance)
                {
                    select = j;
                    break;
                }
            }
            selected = PresentPopData[select];
            return selected;

        }

        //selection
        private genome selection1()//rank selection
        {
            genome selected;
            int[] distarray = new int[popsize];
            for (int i = 0; i < PresentPopData1.Count;i++ )
            {
                distarray[i] = PresentPopData1[i].distance;
            }
            int temp;
            for (int i = 0; i < popsize;i++ )
            {
                for(int j=0;j<popsize;j++)
                {
                    if(distarray[i]<distarray[j])//descending 
                    {
                        temp = distarray[i];
                        distarray[i] = distarray[j];
                        distarray[j] = temp;
                    }
                }
            }
            int r = selecter.Next(0, sum);
            int accumulatefitness = 0;
            int select;
            for(select=10;select>0;select--)//fitness accumulation
            {
                accumulatefitness += select;
                if(accumulatefitness>r)
                {
                    break;
                }
            }
            for (int j = 0; j < popsize;j++ )//find selected one in presentpop
            {
                if(distarray[select-1]==PresentPopData1[j].distance)
                {
                    select = j;
                    break;
                }
            }
            selected = PresentPopData1[select];
            return selected;

        }

        //etilism
        private void etilism()
        {
            int[] max=new int[2];
            max[0]=max[1] = 0;
            for(int i=0;i<PresentPopData.Count;i++)
            {
                if(PresentPopData[max[0]].distance>PresentPopData[i].distance)
                {
                    max[0] = i;
                }
            }
            for (int i = 0; i < PresentPopData.Count; i++)
            {
                if (PresentPopData[max[1]].distance > PresentPopData[i].distance)
                {
                    if (max[0] != i)
                    {
                        max[1] = i;
                    }
                }
            }
            storenewpop(PresentPopData[max[0]]);
            storenewpop(PresentPopData[max[1]]);
        }

        //etilism
        private void etilism1()
        {
            int[] max = new int[2];
            max[0] = max[1] = 0;
            for (int i = 0; i < PresentPopData1.Count; i++)
            {
                if (PresentPopData1[max[0]].distance > PresentPopData1[i].distance)
                {
                    max[0] = i;
                }
            }
            for (int i = 0; i < PresentPopData1.Count; i++)
            {
                if (PresentPopData1[max[1]].distance > PresentPopData1[i].distance)
                {
                    if (max[0] != i)
                    {
                        max[1] = i;
                    }
                }
            }
            storenewpop1(PresentPopData1[max[0]]);
            storenewpop1(PresentPopData1[max[1]]);
        }
        
        //crossover
        private genome[] crossover(genome parent1,genome parent2)
        {
            Random k = new Random();
            int h = k.Next(1, this.cities.Count-2);
            genome[] children = new genome[2];
            children[0] = new genome();
            children[1] = new genome();
            if (crossoverrandom.NextDouble() < crossoverrate)
            {
                for (int i = 0; i < h;i++ )//copy first parts of parent 1 to child 1
                {
                    children[0].order.Add(parent1.order[i]);
                    
                }
                for (int i = h; i < this.cities.Count;i++ )//copy second part of parent 2 to child 2
                {
                    children[1].order.Add(parent2.order[i]);
                }
                    for (int i = 0; i < parent2.order.Count; i++)//fill up child 1
                    {
                        bool found = false;
                        for (int j = 0; j < children[0].order.Count; j++)
                        {
                            if (parent2.order[i] == children[0].order[j])
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found == false)
                        {
                            children[0].order.Add(parent2.order[i]);
                        }
                    }
                for (int i = 0; i < parent1.order.Count; i++)//fill up child 2
                {
                    bool found = false;
                    for (int j = 0; j < children[1].order.Count; j++)
                    {
                        if (parent1.order[i] == children[1].order[j])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        children[1].order.Add(parent1.order[i]);
                    }
                }
                    return children;
            }
            else
            {
                children[0] = parent1;
                children[1] = parent2;
                return children;
            }
        }

        //mutate
        private genome mutate(genome child)
        {
            Random d = new Random();
            int num1 = d.Next(0, child.order.Count - 1);
            int num2=0;
            do
            {
                num2 = d.Next(0, child.order.Count - 1);
            } while (num1 == num2);
            if (mutaterandom.NextDouble() < mutationrate)
            {
                int temp = child.order[num1];
                child.order[num1] = child.order[num2];
                child.order[num2] = temp;
            }
            child.distance = genomefitness(child);
            child.makestring();
            return child;
        }

        //store to new population
        private void storenewpop(genome newgenome)
        {
            NewPopData.Add(newgenome);
        }

        //store to new population
        private void storenewpop1(genome newgenome)
        {
            NewPopData1.Add(newgenome);
        }

        //store new population as current
        private void setcurrpop()
        {
            PresentPopData.Clear();
            for (int i = 0; i < NewPopData.Count;i++ )
            {
                PresentPopData.Add(NewPopData[i]);
            }
            NewPopData.Clear();
        }

        //store new population as current
        private void setcurrpop1()
        {
            PresentPopData1.Clear();
            for (int i = 0; i < NewPopData1.Count; i++)
            {
                PresentPopData1.Add(NewPopData1[i]);
            }
            NewPopData1.Clear();
        }
        
        //display best soln
        private void bestsoln()
        {
            int max = 0;
            for(int i=0;i<popsize;i++)
            {
                if(PresentPopData[max].distance>PresentPopData[i].distance)
                {
                    max = i;
                }
            }
            this.BestGenome.Clear();
            this.BestGenome.Add(PresentPopData[max]);
        }
       
        //display best soln
        private void bestsoln1()
        {
            int max = 0;
            for (int i = 0; i < popsize; i++)
            {
                if (PresentPopData1[max].distance > PresentPopData1[i].distance)
                {
                    max = i;
                }
            }
            this.BestGenome1.Clear();
            this.BestGenome1.Add(PresentPopData1[max]);
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            GA.CancelAsync();
        }

        private void back1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GA1.CancelAsync();
            this.dist.Content = "";
            this.order.Content = "";
            Storyboard f = (Storyboard)TryFindResource("back");
            f.Begin();
        }
    }
}
