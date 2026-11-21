using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
/*
asssumptions
1.i considered each machine as a semaphore (1,1) for each machine A,B,C there is a semaphore

2.Also   a class Machine related to each semaphore 

3.RandomAroundMean to add generart the vay of time 

4. in the main i create 3 semaphore (SA,SB,SC) and created 3 objects for class mahine for each machine( A, B, C)

5.and used datetime to stimuitte the code for 4800 milliseconds

6. there is a widget class which represent the each widget which need to be passed to the factory 

7.on a while loop i create the widget and at the time i pass the object of machine A ,B, C and also passed the semaphore SA,SB,SC and
then pass the same for all the widget 
the widget who get the semaphore of each machine will use that 

8.widget is a task so each task run in paralel but one widget task will run in the order of machine A then Machine B then Machine C 

9.there is a class Sumofcom to store the time each widget spent it the factory for completion the calculation is 
minus of exit time from machine to the time of arrrival .time of arrival is strored as datetime.now at the time of creation of each widget

10. there is a list of waitingwidget is maintatined int he machine class which will keep the queue of the waiting widget it got updated 
eveytime a new widget to access a machine .first it get into the queue and after the execution it is release form the queue 

11.average time spend is calcuted from the count and sum filed in the Sumofcom class 

12.the things in the queue is printed after the 4800 milliseconds  for machine A ,B, C


*/

public class Machine
{
    public int qlength = 0;

    public int totaltime =0;
    public object lockobj = new object();
     public List<int> waitingWidgets = new List<int>();
    public void addQ(int a)
    {
        
        waitingWidgets.Add(a);
        qlength = Math.Max(qlength,waitingWidgets.Count);
    }
    public void remQ(int a)
    {
        qlength = Math.Max(qlength,waitingWidgets.Count);
        waitingWidgets.Remove(a);
    }


    public void use(int time)
    {
        lock (lockobj)
        {
            
            totaltime += time;
        }
    }
    public int Gettimetaken()
    {
            return totaltime;
        

    }
    public int maxQlen()
    {
        return qlength;
    }
}

public class Sumofcom
{
        public object locksum = new object();
    int sum =0;
    int count =0;
    public int Getcount()
    {
        return count;
    }
    public int GetSum()
    {
        return sum;
    }
    public void addcom(int time)
    {
        lock (locksum)
        {
            ++count;
            sum+= time;
        }
    }

}
public class Widget
{
    int ID ;
    DateTime arrivaltime;
    DateTime entryA;

    DateTime entryB;
    DateTime entryC;
    DateTime exitA;
    DateTime exitB;
    DateTime exitC;
    public Machine A ;
    public Machine B ;
    public Machine C ;
    public SemaphoreSlim SA;
    public SemaphoreSlim SB;
    public SemaphoreSlim SC;
    public Sumofcom sum;
    public Widget(int id,Machine A ,Machine B, Machine C, SemaphoreSlim SA, SemaphoreSlim SB, SemaphoreSlim SC, Sumofcom sum)
    {
        this.ID =id;
        arrivaltime = DateTime.Now;
        this.A = A;
        this.B = B;
        this.C = C;
        this.SA = SA;
        this.SB = SB;
        this.SC = SC;
        this.sum =sum;
        Console.WriteLine( $"\nWidget {ID} is created \n");
    }
    public async Task widgetmaking(double a ,double b ,double c )
    {
        lock (A.lockobj) {
             A.addQ(this.ID); 
             }
        Console.WriteLine($"Widget {this.ID} Waiting for Machine A ");
        await SA.WaitAsync();
        entryA = DateTime.Now;
        Console.WriteLine($"widget {this.ID} got machine A at {entryA.TimeOfDay}");
        await Task.Delay((int)a);
        exitA = DateTime.Now;
        SA.Release();
        lock (A.lockobj) { 
            A.remQ(this.ID);
            A.use((int)a) ;
            }
        Console.WriteLine($"Widget {this.ID} releseed machine A at {exitA.TimeOfDay}");
        lock (B.lockobj) {
             B.addQ(this.ID); 
             }
        Console.WriteLine($"Widget {this.ID} Waiting for Machine B ");
        await SB.WaitAsync();
        entryB = DateTime.Now;
        Console.WriteLine($"widget {this.ID} got machine B at {entryB.TimeOfDay}");
        await Task.Delay((int)b);
        SB.Release();
        lock (B.lockobj) { 
            B.remQ(this.ID);
            B.use((int)a) ;
            }
        exitB = DateTime.Now;
        Console.WriteLine($"Widget {this.ID} relseed machine B at {exitB.TimeOfDay}");
        lock (C.lockobj) {
             C.addQ(this.ID);

             }
        Console.WriteLine($"Widget {this.ID} Waiting for Machine C ");
        await SC.WaitAsync();
        entryC = DateTime.Now;
        Console.WriteLine($"widget {this.ID} got machine C at {entryC.TimeOfDay}");
        await Task.Delay((int)c);
        exitC = DateTime.Now;
        SC.Release();
        lock (C.lockobj) { 
            C.remQ(this.ID);
            C.use((int)a) ;
        }
        Console.WriteLine($"Widget {this.ID} relseed machine C at {exitC.TimeOfDay}");
        Console.WriteLine($"\nWidget {this.ID} completed A,B,C   !!!!\n ");
        lock (sum.locksum)
        {
            sum.addcom((int)(exitC - arrivaltime).TotalMilliseconds);
        }
        
    }
    
}

class Program
{
    private static readonly Random _random  = new Random();
public static double RandomAroundMean(double mean ,double percent)
{
    double variation = mean *(percent/100);
    double min = mean - variation;
    double max = mean + variation;
    return min + _random.NextDouble() *(max-min);
}
    static async Task Main()
    {
        using var writer = new StreamWriter("output.txt");
        Console.SetOut(writer);

        var SA = new SemaphoreSlim(1, 1);
        var SB = new SemaphoreSlim(1, 1);
        var SC = new SemaphoreSlim(1, 1);
        var A = new Machine();
        var B = new Machine();
        var C = new Machine();
        var sum = new Sumofcom();

   var startTime = DateTime.Now;
    var tasks = new List<Task>();
    int id = 1;

    while ((DateTime.Now - startTime).TotalMilliseconds < 4800)
    {
        var widget = new Widget(id++, A, B, C, SA, SB, SC,sum);
        tasks.Add(widget.widgetmaking(RandomAroundMean(150,20), RandomAroundMean(300,25), RandomAroundMean(450,60)));
        await Task.Delay((int)RandomAroundMean(300,25));
    }
    Console.WriteLine("Executed for 4800 milliseconds\n");


    Console.WriteLine($"Total time at machine A = {A.Gettimetaken()}");
    Console.WriteLine($"Queue at machine A = [ {string.Join(" ",A.waitingWidgets)} ]");
    Console.WriteLine($"Maximum queue length happend to machine A = {A.maxQlen()}\n");


    Console.WriteLine($"Total time at machine B = {B.Gettimetaken()}");
    Console.WriteLine($"Queue at machine B = [ {string.Join(" ",B.waitingWidgets)} ]");
    Console.WriteLine($"Maximum queue length happend to machine B = {B.maxQlen()}\n");

    Console.WriteLine($"Total time at machine C = {C.Gettimetaken()}");
    Console.WriteLine($"Queue at machine C = [ {string.Join(" ",C.waitingWidgets)} ]");
    Console.WriteLine($"Maximum queue length happend to machine C = {C.maxQlen()}\n");
    
    Console.WriteLine("Total number of fully completed widget = "+ sum.Getcount());
    Console.WriteLine($"Avegrate time spend in factory for each completed widget = {sum.GetSum()/sum.Getcount()}\n" );


    }
}