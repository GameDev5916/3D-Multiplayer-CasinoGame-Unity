#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Nist;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Oiw;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Pkcs;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Encodings;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Engines;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Operators
{
    public class Asn1KeyWrapper
        : IKeyWrapper
    {
        private string algorithm;
        private IKeyWrapper wrapper;

        public Asn1KeyWrapper(string algorithm, X509Certificate cert)
        {
            this.algorithm = algorithm;
            wrapper = KeyWrapperUtil.WrapperForName(algorithm, cert.GetPublicKey());
        }

        public object AlgorithmDetails
        {
            get { return wrapper.AlgorithmDetails; }
        }

        public IBlockResult Wrap(byte[] keyData)
        {
            return wrapper.Wrap(keyData);
        }
    }

    internal class KeyWrapperUtil
    {
        //
        // Provider
        //
        private static readonly IDictionary providerMap = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable();

        static KeyWrapperUtil()
        {
            providerMap.Add("RSA/NONE/OAEPWITHSHA1ANDMGF1PADDING", new RsaOaepWrapperProvider(OiwObjectIdentifiers.IdSha1));
            providerMap.Add("RSA/NONE/OAEPWITHSHA224ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha224));
            providerMap.Add("RSA/NONE/OAEPWITHSHA256ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha256));
            providerMap.Add("RSA/NONE/OAEPWITHSHA384ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha384));
            providerMap.Add("RSA/NONE/OAEPWITHSHA512ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha512));
        }

        public static IKeyWrapper WrapperForName(string algorithm, ICipherParameters parameters)
        {
            WrapperProvider provider = (WrapperProvider)providerMap[Strings.ToUpperCase(algorithm)];

            if (provider == null)
                throw new ArgumentException("could not resolve " + algorithm + " to a KeyWrapper");

            return (IKeyWrapper)provider.CreateWrapper(true, parameters);
        }

        public static IKeyUnwrapper UnwrapperForName(string algorithm, ICipherParameters parameters)
        {
            WrapperProvider provider = (WrapperProvider)providerMap[Strings.ToUpperCase(algorithm)];
            if (provider == null)
                throw new ArgumentException("could not resolve " + algorithm + " to a KeyUnwrapper");

            return (IKeyUnwrapper)provider.CreateWrapper(false, parameters);
        }
    }

    internal interface WrapperProvider
    {
        object CreateWrapper(bool forWrapping, ICipherParameters parameters);
    }

    internal class RsaOaepWrapper : IKeyWrapper, IKeyUnwrapper
    {
        private readonly AlgorithmIdentifier algId;
        private readonly IAsymmetricBlockCipher engine;

        public RsaOaepWrapper(bool forWrapping, ICipherParameters parameters, DerObjectIdentifier digestOid)
        {
            AlgorithmIdentifier digestAlgId = new AlgorithmIdentifier(digestOid, DerNull.Instance);

            this.algId = new AlgorithmIdentifier(
                PkcsObjectIdentifiers.IdRsaesOaep,
                new RsaesOaepParameters(
                    digestAlgId,
                    new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, digestAlgId),
                    RsaesOaepParameters.DefaultPSourceAlgorithm));
            this.engine = new OaepEncoding(new RsaBlindedEngine(), DigestUtilities.GetDigest(digestOid) );
            this.engine.Init(forWrapping, parameters);
        }

        public object AlgorithmDetails
        {
            get { return algId; }
        }

        public IBlockResult Unwrap(byte[] cipherText, int offset, int length)
        {
            return new SimpleBlockResult(engine.ProcessBlock(cipherText, offset, length));
        }

        public IBlockResult Wrap(byte[] keyData)
        {
            return new SimpleBlockResult(engine.ProcessBlock(keyData, 0, keyData.Length));
        }
    }

    internal class RsaOaepWrapperProvider
        : WrapperProvider
    {
        private readonly DerObjectIdentifier digestOid;

        internal RsaOaepWrapperProvider(DerObjectIdentifier digestOid)
        {
            this.digestOid = digestOid;
        }

        object WrapperProvider.CreateWrapper(bool forWrapping, ICipherParameters parameters)
        {
            return new RsaOaepWrapper(forWrapping, parameters, digestOid);
        }
    }
}
#pragma warning restore
#endif
