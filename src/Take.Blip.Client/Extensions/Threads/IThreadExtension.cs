using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Threads
{
    /// <summary>
    /// Provides a service for querying messages from threads.
    /// </summary>
    public interface IThreadExtension
    {
        /// <summary>
        /// Returns all threads, with latest message from each one.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Collection of <see cref="Takenet.Iris.Messaging.Resources.Thread"/>, on descending order of date</returns>
        Task<DocumentCollection> GetThreadsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Return messages from a specific thread
        /// </summary>
        /// <param name="identity">Contact's identity to get message history</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Collection of <see cref="Takenet.Iris.Messaging.Resources.ThreadMessage"/>, on descending order of date</returns>
        /// 
        Task<DocumentCollection> GetThreadAsync(Identity identity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Return messages from a specific thread
        /// </summary>
        /// <param name="identity">Contact's identity to get message history</param>
        /// <param name="take">Max number of messages to retrieve</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Collection of <see cref="Takenet.Iris.Messaging.Resources.ThreadMessage"/>, on descending order of date</returns>
        Task<DocumentCollection> GetThreadAsync(Identity identity, int take, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a link to a given bot's transcript of a past conversation.
        /// </summary>
        /// <param name="identity">Contact's identity to get thread transcript</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Lime.Protocol.Document"/>, containing the link to the transcript</returns>
        /// 
        Task<Document> GetTranscriptionAsync(Identity identity, string accessKey, CancellationToken cancellationToken = default(CancellationToken));
    }
}