# RateLimiter

C# rate limiting utility

## motivation
Create helper to respect Webservices rate limit constraint.

## feature
* Easy to use
* Fully asynchroneous: no need of thread sleep
* Thread safe so you can share time contraints object to rate limit diferent threads using the same resource
* Composable: ability to compose diferent rate limits in one constraint

## Sample usage

###Basic

```C#
    //Create Time constraint: max five times by second
    var timeconstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));

    //Use it
    for(int i=0; i<1000; i++)
    {
        await timeconstraint.Perform(ConsoleIt);
    }       
    
    ....
    private Task ConsoleIt()
    {
        Trace.WriteLine(string.Format("{0:MM/dd/yyy HH:mm:ss.fff}", DateTime.Now));
        return Task.FromResult(0);
    }

```

Output
```
05/23/2016 00:14:44.791
05/23/2016 00:14:44.958
05/23/2016 00:14:44.959
05/23/2016 00:14:44.959
05/23/2016 00:14:44.960
05/23/2016 00:14:45.959
05/23/2016 00:14:45.960
05/23/2016 00:14:45.961
05/23/2016 00:14:45.961
05/23/2016 00:14:45.962
05/23/2016 00:14:46.973
...
```

###Composed

```C#
    //Create first constraint: max five times by second
    var constraint = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
    
    //Create second constraint: one time each 100 ms
    var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
    
    //Compose the two constraints
    var composed = constraint.Compose(constraint2);
    
    //Create Time constraint
    var timeconstraint = TimeLimiter(composed);

    //Use it
    for(int i=0; i<1000; i++)
    {
        await timeconstraint.Perform(ConsoleIt);
    }       
```

Output
```
05/21/2016 23:52:48.573
05/21/2016 23:52:48.682
05/21/2016 23:52:48.809
05/21/2016 23:52:48.922
05/21/2016 23:52:49.024
05/21/2016 23:52:49.575
05/21/2016 23:52:49.685
05/21/2016 23:52:49.810
05/21/2016 23:52:49.942
...
```
