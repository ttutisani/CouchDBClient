﻿using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Information about CouchDB server.
    /// </summary>
    public sealed class ServerInfo
    {
        /// <summary>
        /// Gets welcome message from CouchDB.
        /// </summary>
        public string CouchDB { get; }

        /// <summary>
        /// Gets version of CouchDB server.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// CouchDB vendor information.
        /// </summary>
        public VendorInfo Vendor { get; }

        internal ServerInfo(CouchDBServer.ServerInfoDTO serverInfoDTO)
        {
            if (serverInfoDTO == null)
                throw new ArgumentNullException(nameof(serverInfoDTO));

            CouchDB = serverInfoDTO.CouchDB;
            Version = serverInfoDTO.Version;
            Vendor = new VendorInfo(serverInfoDTO.Vendor);
        }
    }

    /// <summary>
    /// CouchDB vendor information.
    /// </summary>
    public sealed class VendorInfo
    {
        internal VendorInfo(CouchDBServer.ServerInfoDTO.VendorInfoDTO vendorInfoDTO)
        {
            if (vendorInfoDTO == null)
                throw new ArgumentNullException(nameof(vendorInfoDTO));

            Name = vendorInfoDTO.Name;
        }

        /// <summary>
        /// Gets name of CouchDB server vendor.
        /// </summary>
        public string Name { get; }
    }
}
