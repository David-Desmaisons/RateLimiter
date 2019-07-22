# RateLimiter

[![build](https://img.shields.io/appveyor/ci/David-Desmaisons/RateLimiter.svg)](https://ci.appveyor.com/project/David-Desmaisons/RateLimiter)
[![codecov](https://codecov.io/gh/David-Desmaisons/RateLimiter/branch/master/graph/badge.svg)](https://codecov.io/gh/David-Desmaisons/RateLimiter)
[![NuGet Badge](https://buildstats.info/nuget/RateLimiter)](https://www.nuget.org/packages/RateLimiter/)
[![MIT License](https://img.shields.io/github/license/David-Desmaisons/RateLimiter.svg)](https://github.com/David-Desmaisons/RateLimiter/blob/master/LICENSE)

C# client-side rate limiting utility.

http://david-desmaisons.github.io/RateLimiter/

## Motivation
The initial motivation was to create helper to respect Web Services rate limit in client application.
However this helper can also be also in other scenarios where you need to temporally limit the usage of one shared resource.

## Features
* Easy to use
* Fully asynchronous: lower resource usage than thread sleep
* Cancellable via CancellationToken
* Thread safe so you can share time constraints object to rate limit different threads using the same resource
* Composable: ability to compose different rate limits in one constraint

## Installation
```bash
Install-Package RateLimiter -Version 2.0.0
```

## Sample usage

### Basic

RateLimiters are awaitable: the code executed after the await will respect the time constraint:


```C#
    using ComposableAsync;

    // Create Time constraint: max five times by second
    var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));

    // Use it
    for(int i=0; i<1000; i++)
    {
        await timeConstraint;
        Trace.WriteLine(string.Format("{0:MM/dd/yyy HH:mm:ss.fff}", DateTime.Now));
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

### As http DelegatingHandler

```C#
    using System.Net.Http;

    //...
    var handler = TimeLimiter
            .GetFromMaxCountByInterval(60, TimeSpan.FromMinutes(1))
            .AsDelegatingHandler();
    var Client = new HttpClient(handler)
```

### With cancellation token:

```C#
    // Create Time constraint: max three times by second
    var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(3, TimeSpan.FromSeconds(1));
    var cancellationSource = new CancellationTokenSource(1100);

    // Use it
    while(true)
    {
        await timeConstraint.Enqueue(ConsoleIt, cancellationSource.Token);
    }
    
    //....
    private static void ConsoleIt()
    {
        Trace.WriteLine(string.Format("{0:MM/dd/yyy HH:mm:ss.fff}", DateTime.Now));
    }

```

Output
```
07/07/2019 18:09:35.645
07/07/2019 18:09:35.648
07/07/2019 18:09:35.648
07/07/2019 18:09:36.649
07/07/2019 18:09:36.650
07/07/2019 18:09:36.650
```


### Composed

```C#
    // Create first constraint: max five times by second
    var constraint = new CountByIntervalAwaitableConstraint(5, TimeSpan.FromSeconds(1));
    
    / /Create second constraint: one time each 100 ms
    var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(100));
    
    // Compose the two constraints
    var timeConstraint = TimeLimiter.Compose(constraint, constraint2);

    // Use it
    for(int i=0; i<1000; i++)
    {
        await timeConstraint;
        Trace.WriteLine(string.Format("{0:MM/dd/yyy HH:mm:ss.fff}", DateTime.Now));
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
