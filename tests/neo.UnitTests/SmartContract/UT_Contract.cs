using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Neo.UnitTests.SmartContract
{
    [TestClass]
    public class UT_Contract
    {
        [TestMethod]
        public void TestGetAddress()
        {
            byte[] privateKey = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(privateKey);
            KeyPair key = new KeyPair(privateKey);
            Contract contract = Contract.CreateSignatureContract(key.PublicKey);
            byte[] expectedArray = new byte[41];
            expectedArray[0] = (byte)OpCode.PUSHDATA1;
            expectedArray[1] = 0x21;
            Array.Copy(key.PublicKey.EncodePoint(true), 0, expectedArray, 2, 33);
            expectedArray[35] = (byte)OpCode.PUSHNULL;
            expectedArray[36] = (byte)OpCode.SYSCALL;
            Array.Copy(BitConverter.GetBytes(InteropService.Crypto.ECDsaVerify), 0, expectedArray, 37, 4);
            Assert.AreEqual(expectedArray.ToScriptHash().ToAddress(), contract.Address);
        }

        [TestMethod]
        public void TestGetScriptHash()
        {
            byte[] privateKey = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(privateKey);
            KeyPair key = new KeyPair(privateKey);
            Contract contract = Contract.CreateSignatureContract(key.PublicKey);
            byte[] expectedArray = new byte[41];
            expectedArray[0] = (byte)OpCode.PUSHDATA1;
            expectedArray[1] = 0x21;
            Array.Copy(key.PublicKey.EncodePoint(true), 0, expectedArray, 2, 33);
            expectedArray[35] = (byte)OpCode.PUSHNULL;
            expectedArray[36] = (byte)OpCode.SYSCALL;
            Array.Copy(BitConverter.GetBytes(InteropService.Crypto.ECDsaVerify), 0, expectedArray, 37, 4);
            Assert.AreEqual(expectedArray.ToScriptHash(), contract.ScriptHash);
        }

        [TestMethod]
        public void TestCreate()
        {
            byte[] script = new byte[32];
            ContractParameterType[] parameterList = new ContractParameterType[] { ContractParameterType.Signature };
            Contract contract = Contract.Create(parameterList, script);
            Assert.AreEqual(contract.Script, script);
            Assert.AreEqual(1, contract.ParameterList.Length);
            Assert.AreEqual(ContractParameterType.Signature, contract.ParameterList[0]);
        }

        [TestMethod]
        public void TestCreateMultiSigContract()
        {
            byte[] privateKey1 = new byte[32];
            RandomNumberGenerator rng1 = RandomNumberGenerator.Create();
            rng1.GetBytes(privateKey1);
            KeyPair key1 = new KeyPair(privateKey1);
            byte[] privateKey2 = new byte[32];
            RandomNumberGenerator rng2 = RandomNumberGenerator.Create();
            rng2.GetBytes(privateKey2);
            KeyPair key2 = new KeyPair(privateKey2);
            Neo.Cryptography.ECC.ECPoint[] publicKeys = new Neo.Cryptography.ECC.ECPoint[2];
            publicKeys[0] = key1.PublicKey;
            publicKeys[1] = key2.PublicKey;
            publicKeys = publicKeys.OrderBy(p => p).ToArray();
            Contract contract = Contract.CreateMultiSigContract(2, publicKeys);
            byte[] expectedArray = new byte[78];
            expectedArray[0] = (byte)OpCode.PUSH2;
            expectedArray[1] = (byte)OpCode.PUSHDATA1;
            expectedArray[2] = 0x21;
            Array.Copy(publicKeys[0].EncodePoint(true), 0, expectedArray, 3, 33);
            expectedArray[36] = (byte)OpCode.PUSHDATA1;
            expectedArray[37] = 0x21;
            Array.Copy(publicKeys[1].EncodePoint(true), 0, expectedArray, 38, 33);
            expectedArray[71] = (byte)OpCode.PUSH2;
            expectedArray[72] = (byte)OpCode.PUSHNULL;
            expectedArray[73] = (byte)OpCode.SYSCALL;
            Array.Copy(BitConverter.GetBytes(InteropService.Crypto.ECDsaCheckMultiSig), 0, expectedArray, 74, 4);
            CollectionAssert.AreEqual(expectedArray, contract.Script);
            Assert.AreEqual(2, contract.ParameterList.Length);
            Assert.AreEqual(ContractParameterType.Signature, contract.ParameterList[0]);
            Assert.AreEqual(ContractParameterType.Signature, contract.ParameterList[1]);
        }

        [TestMethod]
        public void TestCreateMultiSigRedeemScript()
        {
            byte[] privateKey1 = new byte[32];
            RandomNumberGenerator rng1 = RandomNumberGenerator.Create();
            rng1.GetBytes(privateKey1);
            KeyPair key1 = new KeyPair(privateKey1);
            byte[] privateKey2 = new byte[32];
            RandomNumberGenerator rng2 = RandomNumberGenerator.Create();
            rng2.GetBytes(privateKey2);
            KeyPair key2 = new KeyPair(privateKey2);
            Neo.Cryptography.ECC.ECPoint[] publicKeys = new Neo.Cryptography.ECC.ECPoint[2];
            publicKeys[0] = key1.PublicKey;
            publicKeys[1] = key2.PublicKey;
            publicKeys = publicKeys.OrderBy(p => p).ToArray();
            Action action = () => Contract.CreateMultiSigRedeemScript(0, publicKeys);
            action.Should().Throw<ArgumentException>();
            byte[] script = Contract.CreateMultiSigRedeemScript(2, publicKeys);
            byte[] expectedArray = new byte[78];
            expectedArray[0] = (byte)OpCode.PUSH2;
            expectedArray[1] = (byte)OpCode.PUSHDATA1;
            expectedArray[2] = 0x21;
            Array.Copy(publicKeys[0].EncodePoint(true), 0, expectedArray, 3, 33);
            expectedArray[36] = (byte)OpCode.PUSHDATA1;
            expectedArray[37] = 0x21;
            Array.Copy(publicKeys[1].EncodePoint(true), 0, expectedArray, 38, 33);
            expectedArray[71] = (byte)OpCode.PUSH2;
            expectedArray[72] = (byte)OpCode.PUSHNULL;
            expectedArray[73] = (byte)OpCode.SYSCALL;
            Array.Copy(BitConverter.GetBytes(InteropService.Crypto.ECDsaCheckMultiSig), 0, expectedArray, 74, 4);
            CollectionAssert.AreEqual(expectedArray, script);
        }

        [TestMethod]
        public void TestCreateSignatureContract()
        {
            byte[] privateKey = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(privateKey);
            KeyPair key = new KeyPair(privateKey);
            Contract contract = Contract.CreateSignatureContract(key.PublicKey);
            byte[] expectedArray = new byte[41];
            expectedArray[0] = (byte)OpCode.PUSHDATA1;
            expectedArray[1] = 0x21;
            Array.Copy(key.PublicKey.EncodePoint(true), 0, expectedArray, 2, 33);
            expectedArray[35] = (byte)OpCode.PUSHNULL;
            expectedArray[36] = (byte)OpCode.SYSCALL;
            Array.Copy(BitConverter.GetBytes(InteropService.Crypto.ECDsaVerify), 0, expectedArray, 37, 4);
            CollectionAssert.AreEqual(expectedArray, contract.Script);
            Assert.AreEqual(1, contract.ParameterList.Length);
            Assert.AreEqual(ContractParameterType.Signature, contract.ParameterList[0]);
        }

        [TestMethod]
        public void TestCreateSignatureRedeemScript()
        {
            byte[] privateKey = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(privateKey);
            KeyPair key = new KeyPair(privateKey);
            byte[] script = Contract.CreateSignatureRedeemScript(key.PublicKey);
            byte[] expectedArray = new byte[41];
            expectedArray[0] = (byte)OpCode.PUSHDATA1;
            expectedArray[1] = 0x21;
            Array.Copy(key.PublicKey.EncodePoint(true), 0, expectedArray, 2, 33);
            expectedArray[35] = (byte)OpCode.PUSHNULL;
            expectedArray[36] = (byte)OpCode.SYSCALL;
            Array.Copy(BitConverter.GetBytes(InteropService.Crypto.ECDsaVerify), 0, expectedArray, 37, 4);
            CollectionAssert.AreEqual(expectedArray, script);
        }
    }
}
