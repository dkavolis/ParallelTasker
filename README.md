# Parallel Tasker

**Parallel Tasker** is a plugin for Unity (see [Test](https://github.com/dkavolis/ParallelTasker/blob/master/Unity/ParallelTasker/Assets/Scenes/Test.unity)) and Kerbal Space Program that can execute supplied tasks in parallel between any possible combination of defined times (see Task Timing below). Parallel Tasker is expected to be used through [`ParallelTasker.ParallelTasker`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/ParallelTasker.cs) as a hard dependency for other plugins. Times are determined by script execution orders of [PTSynchronizers](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTSynchronizer.cs) and exported in Asset Bundle [synchronizers.pt](https://github.com/dkavolis/ParallelTasker/blob/master/GameData/ParallelTasker/Assets/synchronizers.pt) (yes, asset bundles also export script execution orders). This offers a very lightweight and consistent method to signalize timings of Update events. In addition, **Parallel Tasker** does not generate any garbage while executing tasks except for creating new ones (removed tasks are reused). If no tasks need to be started or ended at a specific time, then that task controller is unsubscribed from receiving the synchronizer events. All classes are defined in `ParallelTasker` namespace.

## ParallelTasker

The API can be found in a static `ParallelTasker` class.

### Properties

* `bool ResetTasksOnSceneChange`: a property to disable/enable task clearing on scene changes (KSP) to be used in case some tasks rely on `GameObject`s that may no longer be available in different scenes. Default: `true`
  
### Subscriptions

* `bool SubscriptionStatus(PTTimePair taskGroup)`: synchronizer subscription status of task controller that runs at time `taskGroup`.
* `bool SubscriptionStatus(PTUpdateEvent updateEvent, PTEventTime eventTime)`: overload of the above.
* `void Subscribe(PTTimePair timePair, Action handler)`: subscribe `handler` to receive `timePair` update event, remove with `Unsubscribe` otherwise the `handler` may not be garbage collected.
* `void Subscribe(PTUpdateEvent updateEvent, PTEventTime eventTime, Action handler)`: overload of the above.
* `void Unsubscribe(PTTimePair timePair, Action handler)`: unsubscribe `handler` from receiving update events at `timePair` time.
* `void Unsubscribe(PTUpdateEvent updateEvent, PTEventTime eventTime, Action handler)`: overload of the above.

### Tasks

Adding a task requires a start `startTime` and end `endTime` time defined by `PTTimePair`. Removing a specific task only requires its `startTime` since that is where it is registered internally. Note that (for now) the `startTime` is not referenced in `PTTask`.

* `PTTask AddTask(PTTimePair startTime, PTTimePair endTime, PTTask task)`: add a task `task` to be executed in timing group `group`, returns the added task, in this case `task`
* `PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object, object> task, uint period = 1)`: add a task with no `initialize` and `finalize` with execution period `period` to the current task list. Returns the added task.
* `PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object> initializer, Func<object, object> task, uint period = 1)`: add a task with `finalize` with execution period `period` to the current task list. Returns the added task.
* `PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object, object> task, Action<object> finalizer, uint period = 1)`: add a task with no `initialize` with execution period `period` to the current task list. Returns the added task.
* `PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object> initializer, Func<object, object> task, Action<object> finalizer, uint period = 1)`: add a complete task with execution period `period` to the current task list. Returns the added task.
* `bool RemoveTask(PTTimePair startTime, PTTask task)`: remove `task` from `group` task list. Returns success/failure of removal.
* `bool RemoveTask(PTTimePair startTime, Func<object, object> task)`: remove the first task with `PTTask.main == task` from the `group` task list. Returns success/failure of removal.
* `void ClearTasks()`: clear all current tasks.
* `void ClearTasks(PTTimePair startTime)`: clear all tasks starting at `startTime` time.

### Logging

Logs are flushed every frame at `LateUpdate.Start`. Logging API is similar to Unity's but does not contain overloads with `UnityEngine.Object` arguments since Unity API is not thread safe and thus should not be used in multi threading. Use sparingly since most tasks will be executed 10s of times per second.

* `void Debug(object message)`: log a normal `message`, only if compiled with `DEBUG`.
* `void DebugFormat(string format, params object[] args)`: log a formatted `format` message with arguments `args`, only if compiled with `DEBUG`.
* `void Log(object message)`: log a normal `message`.
* `void LogFormat(string format, params object[] args)`: log a formatted `format` message with arguments `args`.
* `void LogError(object message)`: log an error `message`.
* `void LogErrorFormat(string format, params object[] args)`: log a formatted error `format` message with arguments `args`.
* `void LogWarning(object message)`: log a warning `message`.
* `void LogWarningFormat(string format, params object[] args)`: log a formatted warning `format` message with arguments `args`.
* `void LogException(Exception exception)`: log an exception `exception`.

## Task Timing

**Parallel Tasker** consists of 3 Update events [`PTUpdateEvent`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTEventTime.cs) and 9 possible timers [`PTEventTime`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTEventTime.cs). Any possible combination of those values is a valid start or end time for the task. Though mixing `FixedUpdate` with `Update` or `LateUpdate` may not make much sense since they run on different timers. Each combination of timing is guaranteed to finish the tasks ending at that time before starting new ones. If start and end times are equal, the task will be finished in the next Update event. Any task with an end time before start time will finish in the next Update event (i.e. start in `LateUpdate` and end in `Update` or end with a lower `PTEventTime`)

### Update Events

They are standard Unity Update events:

```csharp
public enum PTUpdateEvent
{
    Update,
    LateUpdate,
    FixedUpdate
}
```

### Timers

Current timers are based on [KSP Script Execution Order](https://kerbalspaceprogram.com/api/script_order.html):

```csharp
public enum PTEventTime
{
    Start = -8008,
    Precalc = -101,
    Early = -99,
    Earlyish = -1,
    Normal = 0,
    FashionablyLate = 7,
    FlightIntegrator = 9,
    Late = 19,
    End = 8008
}
```

### Default Dequeue Order

The default dequeue order depends on `Enum.GetValues(Type)` so it may change in the future. It currently is:

1. Update.Normal
2. Update.FashionablyLate
3. Update.FlightIntegrator
4. Update.Late
5. Update.End
6. Update.Start
7. Update.Precalc
8. Update.Early
9. Update.Earlyish
10. LateUpdate.Normal
11. LateUpdate.FashionablyLate
12. LateUpdate.FlightIntegrator
13. LateUpdate.Late
14. LateUpdate.End
15. LateUpdate.Start
16. LateUpdate.Precalc
17. LateUpdate.Early
18. LateUpdate.Earlyish
19. FixedUpdate.Normal
20. FixedUpdate.FashionablyLate
21. FixedUpdate.FlightIntegrator
22. FixedUpdate.Late
23. FixedUpdate.End
24. FixedUpdate.Start
25. FixedUpdate.Precalc
26. FixedUpdate.Early
27. FixedUpdate.Earlyish

Tasks that need to be ended are given priority.

## **Parallel Tasker** Tasks

Each task [`PTTask`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTTasks.cs) can be queued for any of the start and end times and consists of 3 functions, unsigned integer and [`PTTimePair`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTEventTime.cs):

* `Func<object> initialize` (optional): a function that returns `object`, guaranteed to run before any other task function. This can be used to copy data from the main thread to be passed to `main`. It is always executed on the main thread.
* `Func<object, object> main`: a function that takes a single `object` (from `initialize`) as an argument and returns `object`. This function is executed in a different thread and thus should be made thread safe. Guaranteed to run after `initialize` and before `finalize` of this task.
* `Action<object> finalize` (optional): a void function that takes a single `object` (from `main`) as an argument, guaranteed to be executed after `initialize` and `main`. Always executed on the main thread. This can be used to copy data from the thread back to the main thread.
* `uint period`: how often this task is executed, i.e. `period` of 1 will execute this task on every Update event. Default: 1.
* `PTTimePair EndTime`: read only (until dynamic subscription can be made to work consistently) end time of this task (one of `PTUpdateEvent` and `PTEventTime` combinations).
