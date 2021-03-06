﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServiceFabric.Remoting.CustomHeaders
{
    /// <inheritdoc />
    /// <summary>
    /// Custom headers passed on using remoting calls
    /// </summary>
    [Serializable]
    public class CustomHeaders : Dictionary<string, object>
    {
        internal const string CustomHeader = "x-fabric-headers";
        internal const string ReservedHeaderServiceUri = "x-fabric-service";

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public CustomHeaders()
        {
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/></param>
        /// <param name="context">The <see cref="StreamingContext"/></param>
        protected CustomHeaders(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        /// <summary>
        /// Create a new instance based on the current <see cref="RemotingContext"/>
        /// </summary>
        /// <returns>An instance of <see cref="CustomHeaders"/> with headers populated using the current <see cref="RemotingContext"/></returns>
        public static CustomHeaders FromRemotingContext()
        {
            var customHeader = new CustomHeaders();

            foreach (var key in RemotingContext.Keys)
            {
                customHeader.Add(key, RemotingContext.GetData(key));
            }

            return customHeader;
        }

        internal byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        internal static CustomHeaders Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                return (CustomHeaders)bf.Deserialize(stream);
            }
        }
    }
}