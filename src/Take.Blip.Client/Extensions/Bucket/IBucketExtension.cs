using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Bucket
{
    /// <summary>
    /// Allow the storage of documents in the server bucket resource.
    /// </summary>
    public interface IBucketExtension
    {
        /// <summary>
        /// Gets an existing document from the bucket by the id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default(CancellationToken)) where T : Document;

        /// <summary>
        /// Gets the stored documents ids.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetIdsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stores a document in the bucket.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="document">The document.</param>
        /// <param name="expiration">The expiration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task SetAsync<T>(string id, T document, TimeSpan expiration = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) where T : Document;

        /// <summary>
        /// Deletes a document from the bucket.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken));       
    }

    public static class BucketExtensionExtensions
    {
        /// <summary>
        /// Gets an existing text from the bucket by the id.
        /// </summary>
        /// <param name="bucketExtension"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetTextAsync(this IBucketExtension bucketExtension, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = await bucketExtension.GetAsync<PlainText>(id, cancellationToken);
            return document?.Text;
        }

        /// <summary>
        /// Stores a text in the bucket.
        /// </summary>
        /// <param name="bucketExtension"></param>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <param name="expiration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task SetTextAsync(this IBucketExtension bucketExtension, string id, string text, TimeSpan expiration = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) 
            => bucketExtension.SetAsync(id, new PlainText() {Text = text}, expiration, cancellationToken);
    }
}
