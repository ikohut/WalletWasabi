﻿using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWasabi.Converters;
using WalletWasabi.Helpers;
using WalletWasabi.Interfaces;
using WalletWasabi.Logging;

namespace WalletWasabi.ChaumianCoinJoin
{
	[JsonObject(MemberSerialization.OptIn)]
	public class CcjRoundConfig : IConfig
	{
		/// <inheritdoc />
		public string FilePath { get; private set; }

		[JsonProperty(PropertyName = "Denomination")]
		[JsonConverter(typeof(MoneyBtcConverter))]
		public Money Denomination { get; private set; }

		[JsonProperty(PropertyName = "ConfirmationTarget")]
		public int? ConfirmationTarget { get; private set; }

		[JsonProperty(PropertyName = "CoordinatorFeePercent")]
		public decimal? CoordinatorFeePercent { get; private set; }

		[JsonProperty(PropertyName = "AnonymitySet")]
		public int? AnonymitySet { get; private set; }

		[JsonProperty(PropertyName = "InputRegistrationTimeout")]
		public long? InputRegistrationTimeout { get; private set; }

		[JsonProperty(PropertyName = "ConnectionConfirmationTimeout")]
		public long? ConnectionConfirmationTimeout { get; private set; }

		[JsonProperty(PropertyName = "OutputRegistrationTimeout")]
		public long? OutputRegistrationTimeout { get; private set; }

		[JsonProperty(PropertyName = "SigningTimeout")]
		public long? SigningTimeout { get; private set; }

		public CcjRoundConfig()
		{

		}

		public CcjRoundConfig(string filePath)
		{
			SetFilePath(filePath);
		}

		public CcjRoundConfig(Money denomination, int? confirmationTarget, decimal? coordinatorFeePercent, int? anonymitySet, long? inputRegistrationTimeout, long? connectionConfirmationTimeout, long? outputRegistrationTimeout, long? signingTimeout)
		{
			FilePath = null;
			Denomination = Guard.NotNull(nameof(denomination), denomination);
			ConfirmationTarget = Guard.NotNull(nameof(confirmationTarget), confirmationTarget);
			CoordinatorFeePercent = Guard.NotNull(nameof(coordinatorFeePercent), coordinatorFeePercent);
			AnonymitySet = Guard.NotNull(nameof(anonymitySet), anonymitySet);
			InputRegistrationTimeout = Guard.NotNull(nameof(inputRegistrationTimeout), inputRegistrationTimeout);
			ConnectionConfirmationTimeout = Guard.NotNull(nameof(connectionConfirmationTimeout), connectionConfirmationTimeout);
			OutputRegistrationTimeout = Guard.NotNull(nameof(outputRegistrationTimeout), outputRegistrationTimeout);
			SigningTimeout = Guard.NotNull(nameof(signingTimeout), signingTimeout);
		}

		/// <inheritdoc />
		public async Task ToFileAsync()
		{
			AssertFilePathSet();

			string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
			await File.WriteAllTextAsync(FilePath,
			jsonString,
			Encoding.UTF8);
		}

		/// <inheritdoc />
		public async Task LoadOrCreateDefaultFileAsync()
		{
			AssertFilePathSet();

			Denomination = new Money(0.1m, MoneyUnit.BTC);
			ConfirmationTarget = 144; // 1 day
			CoordinatorFeePercent = 0.1m;
			AnonymitySet = 100;
			InputRegistrationTimeout = 604800; // One week
			ConnectionConfirmationTimeout = 60;
			OutputRegistrationTimeout = 60;
			SigningTimeout = 60;

			if (!File.Exists(FilePath))
			{
				Logger.LogInfo<CcjRoundConfig>($"{nameof(CcjRoundConfig)} file did not exist. Created at path: `{FilePath}`.");
			}
			else
			{
				string jsonString = await File.ReadAllTextAsync(FilePath, Encoding.UTF8);
				var config = JsonConvert.DeserializeObject<CcjRoundConfig>(jsonString);

				Denomination = config.Denomination ?? Denomination;
				ConfirmationTarget = config.ConfirmationTarget ?? ConfirmationTarget;
				CoordinatorFeePercent = config.CoordinatorFeePercent ?? CoordinatorFeePercent;
				AnonymitySet = config.AnonymitySet ?? AnonymitySet;
				InputRegistrationTimeout = config.InputRegistrationTimeout ?? InputRegistrationTimeout;
				ConnectionConfirmationTimeout = config.ConnectionConfirmationTimeout ?? ConnectionConfirmationTimeout;
				OutputRegistrationTimeout = config.OutputRegistrationTimeout ?? OutputRegistrationTimeout;
				SigningTimeout = config.SigningTimeout ?? SigningTimeout;
			}

			await ToFileAsync();
		}

		/// <inheritdoc />
		public async Task<bool> CheckFileChangeAsync()
		{
			AssertFilePathSet();

			if (!File.Exists(FilePath))
			{
				throw new FileNotFoundException($"{nameof(CcjRoundConfig)} file did not exist at path: `{FilePath}`.");
			}

			string jsonString = await File.ReadAllTextAsync(FilePath, Encoding.UTF8);
			var config = JsonConvert.DeserializeObject<CcjRoundConfig>(jsonString);

			if (Denomination != config.Denomination)
			{
				return true;
			}
			if (ConfirmationTarget != config.ConfirmationTarget)
			{
				return true;
			}
			if (CoordinatorFeePercent != config.CoordinatorFeePercent)
			{
				return true;
			}
			if (AnonymitySet != config.AnonymitySet)
			{
				return true;
			}
			if (InputRegistrationTimeout != config.InputRegistrationTimeout)
			{
				return true;
			}
			if (ConnectionConfirmationTimeout != config.ConnectionConfirmationTimeout)
			{
				return true;
			}
			if (OutputRegistrationTimeout != config.OutputRegistrationTimeout)
			{
				return true;
			}
			if (SigningTimeout != config.SigningTimeout)
			{
				return true;
			}

			return false;
		}
		
		/// <inheritdoc />
		public void SetFilePath(string path)
		{
			FilePath = Guard.NotNullOrEmptyOrWhitespace(nameof(path), path, trim: true);
		}

		/// <inheritdoc />
		public void AssertFilePathSet()
		{
			if (FilePath == null) throw new NotSupportedException($"{nameof(FilePath)} is not set. Use {nameof(SetFilePath)} to set it.");
		}
	}
}