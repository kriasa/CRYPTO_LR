using RSA.Attacks.Wiener;
using System.Numerics;
using RSA.Algorithms.Models;
using RSA.Attacks.Abstractions;

namespace RSA.Attacks.Abstractions
{
    public interface IWienerAttack
    {
        AttackResult ExecuteAttack(RsaPublicKey publicKey);
    }
}