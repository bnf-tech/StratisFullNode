﻿using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.Crypto;

namespace Stratis.Bitcoin.Features.PoA
{
    /// <summary>Signs and validates signatures of <see cref="PoABlockHeader"/>.</summary>
    public class PoABlockHeaderValidator
    {
        private readonly ILogger logger;

        public PoABlockHeaderValidator(ILoggerFactory factory)
        {
            this.logger = factory.CreateLogger(this.GetType().FullName);
        }

        /// <summary>Signs PoA header with the specified key.</summary>
        /// <param name="key">Key to sign wih.</param>
        /// <param name="header"><see cref="PoABlockHeader"/> to sign.</param>
        public void Sign(Key key, PoABlockHeader header)
        {
            uint256 headerHash = header.GetHash();
            ECDSASignature signature = key.Sign(headerHash);

            header.BlockSignature = new BlockSignature { Signature = signature.ToDER() };
        }

        /// <summary>
        /// Verifies if signature of provided header was created using
        /// private key that corresponds to given public key.
        /// </summary>
        /// <param name="pubKey"><see cref="PubKey"/> of private <see cref="Key"/> used to sign the message.</param>
        /// <param name="header"><see cref="PoABlockHeader"/> to verify signature of.</param>
        /// <returns><c>true</c> if verification successful or <c>false</c> otherwise.</returns>
        public bool VerifySignature(PubKey pubKey, PoABlockHeader header)
        {
            if ((header.BlockSignature == null) || header.BlockSignature.IsEmpty())
            {
                this.logger.LogTrace("(-)[NO_SIGNATURE]");
                return false;
            }

            if (!ECDSASignature.IsValidDER(header.BlockSignature.Signature))
            {
                this.logger.LogTrace("(-)[INVALID_DER]");
                return false;
            }

            ECDSASignature signature = ECDSASignature.FromDER(header.BlockSignature.Signature);

            if (!signature.IsLowS)
            {
                this.logger.LogTrace("(-)[NOT_CANONICAL]");
                return false;
            }

            uint256 headerHash = header.GetHash();
            bool isValidSignature = pubKey.Verify(headerHash, signature);

            return isValidSignature;
        }
    }
}
