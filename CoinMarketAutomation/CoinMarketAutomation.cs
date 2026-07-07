using System;
using System.IO;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

namespace CoinMarketAutomation
{
	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{

			IDms thisDms = engine.GetDms();

			var elements = thisDms.GetElements();
			var filteredElements = elements.Where(element => element.Protocol.Name == "Exercise HTTP CoinMarketCap").ToList();

			var folderName = engine.GetScriptParam(2).Value;

			string fullPath = $"C:\\Skyline DataMiner\\Documents\\{folderName}";

			Directory.CreateDirectory(fullPath);

			filteredElements.ForEach(element =>
			{
				var latestListingsTable = element.GetTable(500).GetData().ToList();

				string csvPath = SecurePath.ConstructSecurePath(
					fullPath,
					$"{element.Name}.csv");


				using (var streamWriter = new StreamWriter(csvPath))
				{

					streamWriter.WriteLine("Instance, Rank, Name, Symbol, Price, Market Cap, Volume 24h, Percent Change 1h, Percent Change 24h, Percent Change 7d, Circulating Supply, Total Supply, Number of Market Pairs, Max Supply, Last Updated");

					latestListingsTable.ForEach(row =>
					{
						streamWriter.WriteLine(string.Join(", ", row.Value));
					});
				}
			});

		}
	}
}
