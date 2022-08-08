#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.OpenSsl
{
	public interface IPasswordFinder
	{
		char[] GetPassword();
	}
}
#pragma warning restore
#endif
