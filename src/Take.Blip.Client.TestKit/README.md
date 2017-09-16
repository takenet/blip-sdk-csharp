This library overrides Lime communication interfaces with in-memory queues (DataFlow BufferBlocks as a matter of fact) so all Blip I/O is faked.

** How to start **

- Create a new library project and install your favorite unit test framework and companions (NUnit 3, Shoudly and NSubstitute are recommended).

- Add to the new test library project a project reference to your chatbot library (or project) 

- Start a new test class and setup a TestHost like below:

// Create a test host using your bot's assembly 
var bot = new TestHost(typeof(<SomeClassInsideBotProject).Assembly); // This constructor has overridable timeouts when receiving messages and notifications 

- Start the test host with your fakes.

// WARNING: All Blip Extensions used by your bot must be faked, since there is no communication at all with Blip hosts
// Some fakes are included on Take.Blip.Client.Testing.Fakes namespace (for instance, MemoryBucketExtension)
Bucket = new MemoryBucketExtension();
await bot.AddRegistrationAndStartAsync(Bucket);

- Deliver a message to your bot, faking an user interaction.

// Dummy is a helper class to create formatted Lime Nodes as Blip produces
var messengerUser = Dummy.MessengerUser();

await bot.DeliverIncomingMessageAsync(messengerUser, "Hello"); // Plain text as well other Blip contents could be sent

- Get bot Message response (and make assertions based on your bot's conversation flow)

var response = await bot.RetrieveOutgoingMessageAsync();

- Repeat deliver and retrieve methods until you have tested the intended conversation flow

- Stop test host and dispose objects
await bot.StopAsync();
Bucket.Dispose();

** Helpers **

There is some helper extensions:

- WaitForConsumedAsync(): Waiting for internal notifications sent by Blip SDK after a message is fully processed. 
Useful when asserting interaction with (mocked) services used inside Message Receivers, mainly when a response is produced before these services are called. 

Usage:

await bot.WaitForConsumedAsync();

- GetResponseIgnoreFireAndForgetAsync(): When your bot send fire and forget messages, for instance when using ChatState to send typing indicators, this method
will ignore them and return the next message instead.

Usage:

var response = await bot.GetResponseIgnoreFireAndForgetAsync();


