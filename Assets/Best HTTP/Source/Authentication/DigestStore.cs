﻿using System;
using System.Collections.Generic;

namespace BestHTTP.Authentication
{
    /// <summary>
    /// Stores and manages already received digest infos.
    /// </summary>
    public static class DigestStore
    {
        private static Dictionary<string, Digest> Digests = new Dictionary<string, Digest>();

        private static System.Threading.ReaderWriterLockSlim rwLock = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Array of algorithms that the plugin supports. It's in the order of priority(first has the highest priority).
        /// </summary>
        private static string[] SupportedAlgorithms = new string[] { "digest", "basic" };

        public static Digest Get(Uri uri)
        {
            rwLock.EnterReadLock();
            try{
                Digest digest = null;
                if (Digests.TryGetValue(uri.Host, out digest))
                    if (!digest.IsUriProtected(uri))
                        return null;
                return digest;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// It will retrieve or create a new Digest for the given Uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Digest GetOrCreate(Uri uri)
        {
            rwLock.EnterUpgradeableReadLock();
            try{
                Digest digest = null;
                if (!Digests.TryGetValue(uri.Host, out digest))
                {
                    rwLock.EnterWriteLock();
                    try
                    {
                        Digests.Add(uri.Host, digest = new Digest(uri));
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                return digest;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        public static void Remove(Uri uri)
        {
            rwLock.EnterWriteLock();
            try
            {
                Digests.Remove(uri.Host);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static string FindBest(List<string> authHeaders)
        {
            if (authHeaders == null || authHeaders.Count == 0)
                return string.Empty;

            List<string> headers = new List<string>(authHeaders.Count);
            for (int i = 0; i < authHeaders.Count; ++i)
                headers.Add(authHeaders[i].ToLower());

            for (int i = 0; i < SupportedAlgorithms.Length; ++i)
            {
                int idx = headers.FindIndex((header) => header.StartsWith(SupportedAlgorithms[i]));
                if (idx != -1)
                    return authHeaders[idx];
            }

            return string.Empty;
        }
    }
}