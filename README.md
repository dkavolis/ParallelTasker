# Parallel Tasker

**Parallel Tasker** is a plugin for Unity (see [Test](https://github.com/dkavolis/ParallelTasker/blob/master/Unity/ParallelTasker/Assets/Scenes/Test.unity)) and Kerbal Space Program that can execute supplied tasks in parallel while one of Unity's Update events are running or in between starts of an Update event. Parallel Tasker is expected to be used through [`ParallelTasker.ParallelTasker`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/ParallelTasker.cs) as a hard dependency for other plugins. Start and end of each Update event is determined by [PTSynchronizers](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTSynchronizer.cs) - start has execution order of -8000 and end - 8000 (in between KSP Timing0 and Timing5 scripts) and exported in Asset Bundle [synchronizers.pt](https://github.com/dkavolis/ParallelTasker/blob/master/GameData/ParallelTasker/Assets/synchronizers.pt) (yes, setting the Script Execution Order from the Unity Editor and exporting synchronizers works - try swapping the order and see all tasks executed in between Update events). This offers a very lightweight and consistent method to signalize start and end timings of Update events. In addition, **Parallel Tasker** is not expected to generate any garbage while executing tasks. All classes are defined in `ParallelTasker` namespace.

## ParallelTasker

The API can be found in a static `ParallelTasker` class and contains methods for adding/removing tasks and thread safe logging. All logs are guaranteed to be flushed after all the tasks in the corresponding timing group have finished. Logging API is similar to Unity's but does not contain overloads with `UnityEngine.Object` arguments since Unity API is not thread safe and thus should not be used in multi threading. For an explanation of timing groups and tasks see below.

* `bool ResetTasksOnSceneChange`: a property to disable/enable task clearing on scene changes (KSP) to be used in case some tasks rely on `GameObject`s that may no longer be available in different scenes. Default: `true`
* `PTTask AddTask(PTGroup group, PTTask task)`: add a task `task` to be executed in timing group `group`, returns the added task, in this case `task`
* `PTTask AddTask(PTGroup group, Func<object, object> task)`: add a task with no `initialize` and `finalize` to the current task list. Returns the added task.
* `PTTask AddTask(PTGroup group, Func<object> initializer, Func<object, object> task)`: add a task with `finalize` to the current task list. Returns the added task.
* `PTTask AddTask(PTGroup group, Func<object, object> task, Action<object> finalizer)`: add a task with no `initialize` to the current task list. Returns the added task.
* `PTTask AddTask(PTGroup group, Func<object> initializer, Func<object, object> task, Action<object> finalizer)`: add a complete task to the current task list. Returns the added task.
* `bool RemoveTask(PTGroup group, PTTask task)`: remove `task` from `group` task list. Returns success/failure of removal.
* `bool RemoveTask(PTGroup group, Func<object, object> task)`: remove the first task with `PTTask.main == task` from the `group` task list. Returns success/failure of removal.
* `void Log(PTGroup group, object message)`: log a normal `message` in `group` logger.
* `void LogFormat(PTGroup group, string format, params object[] args)`: log a formatted `format` message with arguments `args` in `group` logger.
* `void LogError(PTGroup group, object message)`: log an error `message` in `group` logger.
* `void LogErrorFormat(PTGroup group, string format, params object[] args)`: log a formatted error `format` message with arguments `args` in `group` logger.
* `void LogWarning(PTGroup group, object message)`: log a warning `message` in `group` logger.
* `void LogWarningFormat(PTGroup group, string format, params object[] args)`: log a formatted warning `format` message with arguments `args` in `group` logger.
* `void LogException(PTGroup group, Exception exception)`: log an exception `exception` in `group` logger.
* `void ClearTasks()`: clear all current tasks.
* `void ClearTasks(PTGroup group)`: clear all `group` tasks.

## Groups

**Parallel Tasker** consists of 6 timing groups in [`PTGroup`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTGroup.cs):

1. `Update` - started at the start of Update event and finished at the end of the same event.
2. `LateUpdate` - started at the start of LateUpdate event and finished at the end of the same event.
3. `FixedUpdate` - started at the start of FixedUpdate event and finished at the end of the same event.
4. `UpdateFrame` - started at the start of Update event and finished at the start of next the Update event.
5. `LateUpdateFrame` - started at the start of LateUpdate event and finished at the start of next the LateUpdate event.
6. `FixedUpdateFrame` - started at the start of FixedUpdate event and finished at the start of next the FixedUpdate event.

Tasks in `Frame` groups are started after and finished before corresponding single event tasks (i.e. `UpdateFrame` tasks are finished, `Update` tasks are started, `UpdateFrame` tasks are started, normal Unity execution resumes). Execution order in parallel is not guaranteed and tasks running in parallel should not rely on other tasks. By default, if no priority is given to any of the groups, tasks will be dequeued in the order given here.

## Tasks

Each task [`PTTask`](https://github.com/dkavolis/ParallelTasker/blob/master/ParallelTasker/PTTasks.cs) can be queued for any of the timing groups and consists of 3 functions:

* `Func<object> initialize`: a function that returns `object`, guaranteed to run before any other task function. This can be used to copy data from the main thread to be passed to `main`. It is always executed on the main thread.
* `Func<object, object> main`: a function that takes a single `object` (from `initialize`) as an argument and returns `object`. This function is executed in a different thread and thus should be made thread safe. Guaranteed to run after `initialize` and before `finalize` of this task.
* `Action<object> finalize`: a void function that takes a single `object` (from `main`) as an argument, guaranteed to be executed after `initialize` and `main`. Always executed on the main thread. This can be used to copy data from the thread back to the main thread.
