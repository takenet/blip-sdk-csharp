Using ClearScript V8 to execute JavaScript as a new action.

It doesn't changes the behavior of the current ExecuteScriptAction.

The new implementation allow users to use recent ECMAScript definitions with way less restrictions.

Additionally, users can execute HTTP request inside the JavaScript with a custom `request.fetchAsync` API.

The new action also deals with user returns and parse it manually to store in the variable, awaiting any promises used by the JS script.

Code coverage: 87%:
![image](https://github.com/takenet/blip-sdk-csharp/assets/10624972/65ad2db3-e4d0-42bd-b9f9-1572b78dfe67)

Benchmark:

| Method                      | Mean         | Error     | StdDev    | Gen0     | Gen1     | Allocated   |
|---------------------------- |-------------:|----------:|----------:|---------:|---------:|------------:|
| ExecuteScriptV1SimpleScript |     27.90 us |  0.170 us |  0.159 us |   2.8687 |   0.7324 |    35.36 KB |
| ExecuteScriptV1JsonScript   |    104.94 us |  0.208 us |  0.162 us |   8.0566 |   2.0752 |   100.15 KB |
| ExecuteScriptV1MathScript   |    288.78 us |  0.838 us |  0.700 us |  20.5078 |   5.3711 |   251.85 KB |
| ExecuteScriptV2SimpleScript |  1,435.43 us | 11.332 us | 10.600 us |   3.9063 |   1.9531 |    53.99 KB |
| ExecuteScriptV2JsonScript   |  1,483.13 us | 26.174 us | 23.203 us |   3.9063 |   1.9531 |    53.99 KB |
| ExecuteScriptV2MathScript   |  1,527.14 us | 21.611 us | 20.215 us |   3.9063 |   1.9531 |    54.03 KB |
| ExecuteScriptV2LoopScript   |  1,545.27 us | 19.645 us | 18.376 us |   3.9063 |   1.9531 |    54.54 KB |
| ExecuteScriptV1LoopScript   | 12,339.65 us | 64.323 us | 60.168 us | 859.3750 | 203.1250 | 10697.74 KB |

> The scripts used for the tests are in [here](https://github.com/takenet/blip-sdk-csharp/blob/99118c1cf06822546004a7610aafc6a930f1b3c7/src/Take.Blip.Builder.Benchmark/Actions/Settings.cs).
>
> I had to remove the limt of max statements to execute the loop script in the V1 version, because it was getting blocked.

Conclusions I made from the benchmark:
- `Execution Time`: V1 is consistently faster than V2 depending on the complexity of the JS script. V2 seems to have a constant overhead regardless of script complexity, which can be good for us to have our system stable regardless of what our users create.

- `Memory Allocation`: V2 allocates less memory (Gen0, Gen1, Allocated) compared to V1 and seems to have better memory management. V1 only allocated less memory executing the simplest script, but allocated 10MB of memory for a 1000-interactions loop doing absolutelly nothing, which can be the reason we had to set the MaxStatements in the past.

In summary I think V2 is more stable than V1 but it always take some time to configure and execute. In the future we may need to investigate what in the ExecuteScriptV2Action is spending more time.

Removing the RegisterFunctions and the result ScriptObjectConverter just to make sure it is not affecting the results:

| Method                      | Mean         | Error      | StdDev     | Gen0     | Gen1     | Allocated   |
|---------------------------- |-------------:|-----------:|-----------:|---------:|---------:|------------:|
| ExecuteScriptV1SimpleScript |     28.80 us |   0.077 us |   0.065 us |   2.8687 |   0.7324 |    35.36 KB |
| ExecuteScriptV1JsonScript   |    101.78 us |   1.200 us |   1.122 us |   8.0566 |   2.0752 |   100.15 KB |
| ExecuteScriptV1MathScript   |    290.85 us |   2.210 us |   2.067 us |  20.5078 |   5.3711 |   251.85 KB |
| ExecuteScriptV2SimpleScript |  1,267.44 us |  16.294 us |  15.242 us |   1.9531 |        - |     26.7 KB |
| ExecuteScriptV2JsonScript   |  1,356.35 us |  26.873 us |  40.223 us |   1.9531 |        - |    26.72 KB |
| ExecuteScriptV2MathScript   |  1,388.88 us |  19.683 us |  20.213 us |   1.9531 |        - |    26.82 KB |
| ExecuteScriptV2LoopScript   |  1,390.48 us |  20.609 us |  19.277 us |   1.9531 |        - |    27.24 KB |
| ExecuteScriptV1LoopScript   | 11,653.97 us | 125.647 us | 117.530 us | 859.3750 | 218.7500 | 10697.74 KB |

For now I think the memory optimizations may have a good impact in our systems, even taking 1~1.5ms more to execute scripts.

# Features

The following documents all the functions available to use inside the JS:

# Exceptions

The new version have a new settings to capture all exceptions when executing the script.

When capturing exceptions, you may store the exception message in a variable to handle it in the flow.

Our internal exceptions are also captured but the message may be redacted for security reasons. A trace id will be provided in these casees and may be used to get more information about the exception logged by the system.

We also add a warning to the trace execution with the exception message, so you can see the error in the trace (in the Beholder extension when using Blip's Builder, for example).

If the variable name to store exception is not provided, the exception will be captured, the trace will have a warning message and the script will continue to execute normally without setting the variable.

```csharp
/// <summary>
/// If the script should capture all exceptions instead of throwing them.
/// </summary>
public bool CaptureExceptions { get; set; }

/// <summary>
/// The variable to store the exception message if CaptureExceptions is true.
/// </summary>
public string ExceptionVariable { get; set; }
```

# Time Manipulation Limitations

Although the new implementation supports more recent time manipulation functions like `dateVariable.toLocaleString('pt-BR', { timeZone: 'America/Sao_Paulo' })`, we have some limitations with the `ExecuteScriptV2Action`.

## Limitations:

- Differently from the `ExecuteScriptAction`, we do not set the local timezone for the script engine based on bot's configured timezone and only modified only three functions from date's prototype to match bot's timezone: `Date.prototype.toDateString`, `Date.prototype.toTimeString` and `Date.prototype.toString`. It will use, when available (`builder:#localTimeZone` in the flow configuration`), the timezone of the configured bot.

The problem is using the native constructor to parse strings without timezone in its format. It will use the local server engine timezone by default. Because of that we suggest you to use `time.parseDate` instead.

Example:

```
// Instead of this:
const date = new Date('2021-01-01T00:00:10'); // will use server's local timezone
// use this:
const date = time.parseDate('2021-01-01T00:00:10'); // will use bot's timezone or fixed America/Sao_Paulo if not available.
```

- If you return a `Date` object in the script result, it will be stored in the context variable with the following format: `yyyy-MM-dd'T'HH:mm:ss.fffffffK`, which is the same of the default format used by the `time.dateToString` helper.
  If you need to parse the date in another script using the variable as input, you have two options:
    - Parse the date using the format above with javascript's native date constructor, since it contains the timezone in the string.
    - Parse the date using our helper `time.parseDate` documented below.
    - Return the string representation of the date in the first script with your own desired format using `time.dateToString` or native's date formats and then parse it in the other script you are using it as variable. Make sure to include the timezone in the string format if you choose a custom format.

> If you want to convert the date to string format with custom timezone, you may use `date.toLocaleString` from native JS and use the options to configure the timezone yourself, or our helper `time.dateToString`, both accepts the timeZone as an option.
>
> If you want to parse a string representation of a date that doesn't includes a timezone, but want to specify a custom timezone, use our `time.parseDate` helper and pass the `timeZone` in the options.

Date and time helpers:

> To configure a bot's timezone, the flow configuration must have the following key: `builder:#localTimeZone`.
>
> Also, the action settings `LocalTimeZoneEnabled` must be `true`.

### ParseDate

`time.parseDate(string date, object? options)`

Function to parse a date string to a DateTime object.

It receives two parameters, with the last one being optional:
- `date`: The date string to be parsed.
- `options`: optional parameter to configure options when parsing the date. Available options:
    - `format`: Optional parameter to set the format of the date string.
      If not provided it will try to infer the format, which may fail depending on the string.
    - `culture`: Optional parameter to infer the culture used in the string format.
      It will use `en-US` if not set.
    - `timeZone`: Optional parameter to define which time zone should be used if the string doesn't includes the timezone in the format. If not set it will try to use bot's configured timezone or, if not available, `America/Sao_Paulo`.

`time.parseDate` returns a JS Date object.

Examples:

```js
const date = time.parseDate('2021-01-01T19:01:01.0000001+08:00');

const date = time.parseDate('01/02/2021', {format: 'MM/dd/yyyy'});

const date = time.parseDate('2021-01-01 19:01:01', {format:'yyyy-MM-dd HH:mm:ss', timeZone: 'America/New_York');

const date = time.parseDate('01/01/2021', {format: 'dd/MM/yyyy', culture: 'pt-BR'});
```

> After parsing the date, it will return a JS Date object, and you are freely to manipulate it as you want.

### DateToString

`time.dateToString(Date date, object? options)`

Function to convert a Date object to a string using the bot's configured timezone or `America/Sao_Paulo` if not set.

It receives two parameters:
- `date`: The Date object to be converted.
- `options`: optional parameter to configure options when formatting the date. Available options:
    - `format`: Optional parameter to set the format of the date string.
      If not provided it will use `yyyy-MM-dd'T'HH:mm:ss.fffffffK`.
    - `timeZone`: Optional parameter to define which time zone should be used to format the string. If not set it will try to use bot's configured timezone or, if not available, `America/Sao_Paulo`.

It returns a string.

Examples:

```js
const dateString = time.dateToString(new Date());

const dateString = time.dateToString(time.parseDate('2021-01-01T19:01:01.0000001+08:00'));

const dateString = time.dateToString(new Date(), {format: "yyyy-MM-dd"});

const dateString = time.dateToString(new Date(), {timeZone: "America/New_York"});
```

### Sleep

Additionally, we have a helper function to sleep the script execution for a given amount of time:

`time.sleep(int milliseconds)`

> Use this with caution, as it will block the script execution for the given amount of time and make your bot's execution slower.
>
> Remember that all the scripts have a maximum execution time and may throw an error if the script takes too long to execute.

It receives one parameter:
- `milliseconds`: The amount of time to sleep in milliseconds.

Examples:

```js
time.sleep(50);
```

> It is useful to force your script to timeout and test how your bot behaves when the script takes too long to execute.

# Context

We added some functions to interact with the context variables inside the script.

## Set Variable

`context.setVariableAsync(string name, object value, TimeSpan? expiration)`

Async function to set variable to the context. Should be used in async context.

It receives three parameters, with the last one being optional:
- `name`: The name of the variable to be set.
- `value`: The value to be set. Can be any object that will be serialized to use as the variable value.
- `expiration`: Optional parameter to set the expiration time of the variable.
  If not set, the variable will not expire.
    - TimeSpan: you can use the TimeSpan class to set the expiration time.

Examples:

```js
async function run() {
    await context.setVariableAsync('myVariable', 'myValue');
    // Variable value: 'myValue'

    await context.setVariableAsync('myVariable', 'myValue', TimeSpan.fromMinutes(5));
    // Variable value: 'myValue'

    await context.setVariableAsync('myVariable', 100, TimeSpan.fromMilliseconds(100));
    // Variable value: '100'

    await context.setVariableAsync('myVariable', {'complex': true}, TimeSpan.fromMilliseconds(100));
    // Variable value: '{"complex":true}'
}
```
## Get Variable

`context.getVariableAsync(string name)`

Async function to get variable from the context. Should be used in async context.

Returns an empty string if the variable does not exist.

It receives one parameter:
- `name`: The name of the variable to be retrieved.

Examples:

```js
async function run() {
    const emptyVariable = await context.getVariableAsync('myVariable');
    // emptyVariable: ''

    await context.setVariableAsync('myVariable', 'myValue');

    const myVariable = await context.getVariableAsync('myVariable');
    // myVariable: 'myValue'
}
```

## Delete Variable

`context.deleteVariableAsync(string name)`

Async function to delete variable from the context. Should be used in async context.

It receives one parameter:
- `name`: The name of the variable to be deleted.

Examples:

```js
async function run() {
    await context.setVariableAsync('myVariable', 'myValue');

    await context.deleteVariableAsync('myVariable');

    const myVariable = await context.getVariableAsync('myVariable');
    // myVariable: ''
}
```

# HTTP Requests

We added a new fetch API to allow users to make HTTP requests inside the script.

## Fetch API

`request.fetchAsync(string url, object? options)`

An Async function to make HTTP requests inside the script. Should be used in async context.

It receives two parameters, with the last one being optional:
- `url`: The URL to make the request.
- `options`: Optional parameter to set the request options. It must be a dictionary with the following optional properties:
    - `method`: The HTTP method to be used. Default: 'GET'.
    - `headers`: The headers to be sent with the request. Default: {}. Header values in the request options can be a string or an array of strings.
    - `body`: The body to be sent with the request. Default: null.

It returns a object with the following properties:
- `status`: The status code of the response.
- `headers`: The headers of the response. It is a dictionary with the header names as keys and the header values as values, where the values are arrays of strings.
- `body`: The body of the response in string format.
- `success`: A boolean indicating if the request was successful (200-299 status code range).

The response also have the `jsonAsync()` method to parse the body as JSON.

> If the request takes too long to execute, it will throw an error according to the script timeout, always remember to handle the errors outside the script in your bot.

Examples:

```js
async function run() {
    const response = await request.fetchAsync('https://jsonplaceholder.typicode.com/todos/1');
    /* response example:
      {
        status: 200,
        headers: { 'key1': ['value1', 'value2'] },
        body: '{"userId": 1, "id": 1, "title": "delectus aut autem", "completed": false}',
        success: true
      }
    */

    const json = await response.jsonAsync();
    // json: { userId: 1, id: 1, title: 'delectus aut autem', completed: false }
}
```

```js
async function run() {
    // Body will be serialized to JSON automatically
    const response = await request.fetchAsync('https://jsonplaceholder.typicode.com/posts', { method: 'POST', body: { title: 'foo', body: 'bar', userId: 1 } });
    /* response example:
      {
        status: 201,
        headers: { 'key1': ['value1', 'value2'] },
        body: '{"title": "foo", "body": "bar", "userId": 1, "id": 101}',
        success: true
      }
    */

    const json = await response.jsonAsync();
    // json: { title: 'foo', body: 'bar', userId: 1, id: 101 }
}
```

```js
// More on object manipulation
async function run() {
    const response = await request.fetchAsync('https://jsonplaceholder.typicode.com/todos/1');
    
    const status = response.status;
    // status: 200
    
    const success = response.success;
    // success: true
    
    const headers = response.headers;
    // headers: { 'key1': ['value1', 'value2'] }
    
    const body = response.body;
    // body: '{"userId": 1, "id": 1, "title": "delectus aut autem", "completed": false}'

    const jsonBody = await response.jsonAsync();
    // jsonBody: { userId: 1, id: 1, title: 'delectus aut autem', completed: false }

    // Header manipulation
    for (const header of response.headers) {
        // do something with header key and value, where value is an array of strings
        // key will always be on lower case
        const key = header.key;
        const values = header.value;

        for (const value of values) {
            // do something with the value
        }
    }

    // Get header with case insensitive key and first value
    // Returns undefined if the header does not exist and an array of strings with the values otherwise
    const contentType = response.getHeader('content-type');
}
```