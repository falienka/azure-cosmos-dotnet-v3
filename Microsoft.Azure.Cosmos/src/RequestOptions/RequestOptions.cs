﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Documents;

    /// <summary>
    /// The default cosmos request options
    /// </summary>
    public class RequestOptions
    {
        internal Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Gets or sets the If-Match (ETag) associated with the request in the Azure Cosmos DB service.
        /// </summary>
        /// <remarks>
        /// Most commonly used with the Delete* and Replace* methods of <see cref="Container"/> such as <see cref="Container.ReplaceItemAsync{T}(T, string, PartitionKey?, ItemRequestOptions, System.Threading.CancellationToken)"/>
        /// but can be used with other methods like <see cref="Container.ReadItemAsync{T}(string, PartitionKey, ItemRequestOptions, System.Threading.CancellationToken)"/> for caching scenarios.
        /// </remarks>
        public string IfMatchEtag { get; set; }

        /// <summary>
        /// Gets or sets the If-None-Match (ETag) associated with the request in the Azure Cosmos DB service.
        /// </summary>
        /// <remarks>
        /// Most commonly used to detect changes to the resource
        /// </remarks>
        public string IfNoneMatchEtag { get; set; }

        /// <summary>
        /// Application opted Cosmos request headers which are sent to Cosmos service part of request execution.
        /// Provided headers are the final values for the headers.
        /// </summary>
        /// <remarks>
        /// Only Cosmos service supported request headers will be sent to service, others might get discarded.
        /// </remarks>
#if PREVIEW
        public
#else
        internal
#endif
        IReadOnlyDictionary<string, string> CustomRequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the boolean to use effective partition key routing in the cosmos db request.
        /// </summary>
        internal bool IsEffectivePartitionKeyRouting { get; set; }

        /// <summary>
        /// Gets or sets the consistency level required for the request in the Azure Cosmos DB service.
        /// Not every request supports consistency level. This allows each child to decide to expose it
        /// and use the same base logic
        /// </summary>
        /// <value>
        /// The consistency level required for the request.
        /// </value>
        /// <remarks>
        /// ConsistencyLevel compatibility will validated and set by RequestInvokeHandler
        /// </remarks>
        internal virtual ConsistencyLevel? BaseConsistencyLevel { get; set; }

        /// <summary>
        /// This allows user to pass in a custom factory for the diagnostic context.
        /// A custom implementation can ignore certain calls to avoid additional overhead
        /// when the information is not required.
        /// </summary>
        internal Func<CosmosDiagnosticsContext> DiagnosticContextFactory { get; set; }

        /// <summary>
        /// Fill the CosmosRequestMessage headers with the set properties
        /// </summary>
        /// <param name="request">The <see cref="RequestMessage"/></param>
        internal virtual void PopulateRequestOptions(RequestMessage request)
        {
            if (this.Properties != null)
            {
                foreach (KeyValuePair<string, object> property in this.Properties)
                {
                    request.Properties[property.Key] = property.Value;
                }
            }

            if (this.IfMatchEtag != null)
            {
                request.Headers.Add(HttpConstants.HttpHeaders.IfMatch, this.IfMatchEtag);
            }

            if (this.IfNoneMatchEtag != null)
            {
                request.Headers.Add(HttpConstants.HttpHeaders.IfNoneMatch, this.IfNoneMatchEtag);
            }

            // Include custom headers if-any
            if (this.CustomRequestHeaders != null)
            {
                foreach (KeyValuePair<string, string> entry in this.CustomRequestHeaders)
                {
                    request.Headers.Add(entry.Key, entry.Value);
                }
            }
        }

        /// <summary>
        /// Gets the resource URI passed in as a request option. This is used by MongoDB and Cassandra implementation for performance reasons.
        /// </summary>
        /// <param name="resourceUri">The URI passed in from the request options</param>
        /// <returns>True if the object exists in the request options. False if the value was not passed in as a request option</returns>
        internal bool TryGetResourceUri(out Uri resourceUri)
        {
            if (this.Properties != null && this.Properties.TryGetValue(HandlerConstants.ResourceUri, out object requestOptesourceUri))
            {
                Uri uri = requestOptesourceUri as Uri;
                if (uri == null || uri.IsAbsoluteUri)
                {
                    throw new ArgumentException(HandlerConstants.ResourceUri + " must be a relative Uri of type System.Uri");
                }

                resourceUri = uri;
                return true;
            }

            resourceUri = null;
            return false;
        }

        /// <summary>
        /// Set the session token
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="sessionToken">The current session token.</param>
        internal static void SetSessionToken(RequestMessage request, string sessionToken)
        {
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                request.Headers.Add(HttpConstants.HttpHeaders.SessionToken, sessionToken);
            }
        }
    }
}
