﻿using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core;
using N2.Persistence;
using N2.Web;
using N2.Definitions;

namespace N2.Globalization
{
	public class LanguageInterceptor : IStartable
	{
		private readonly IPersister persister;
		private readonly IDefinitionManager definitions;
		private readonly IWebContext context;
		private readonly ILanguageGateway gateway;

		public LanguageInterceptor(IPersister persister, IDefinitionManager definitions, IWebContext context, ILanguageGateway gateway)
		{
			this.persister = persister;
			this.definitions = definitions;
			this.context = context;
			this.gateway = gateway;
		}

		#region IStartable Members

		public void Start()
		{
			persister.ItemSaved += persister_ItemSaved;
			persister.ItemMoved += persister_ItemMoved;
			persister.ItemDeleted += persister_ItemDeleted;
			definitions.ItemCreated += definitions_ItemCreated;
		}

		void definitions_ItemCreated(object sender, ItemEventArgs e)
		{
			if (context.QueryString[LanguageGateway.LanguageKey] != null)
			{
				int languageKey;
				if (int.TryParse(context.QueryString[LanguageGateway.LanguageKey], out languageKey))
				{
					ContentItem translation = persister.Get(languageKey);
					e.AffectedItem.SortOrder = translation.SortOrder;
				}
			}
		}

		void persister_ItemDeleted(object sender, ItemEventArgs e)
		{
			ContentItem item = e.AffectedItem;
			foreach (ContentItem translatedItem in gateway.FindTranslations(item))
			{
				persister.Delete(translatedItem);
			}
		}

		void persister_ItemMoved(object sender, DestinationEventArgs e)
		{
			ContentItem item = e.AffectedItem;
			ILanguage language = gateway.GetLanguageAncestor(item);
			
			if (language != null)
			{
				ContentItem destination = e.Destination;
			
				foreach (ContentItem translatedItem in gateway.FindTranslations(item))
				{
					ILanguage translationsLanguage = gateway.GetLanguageAncestor(translatedItem);
					ContentItem translatedDestination = gateway.GetTranslation(destination, translationsLanguage);
					if (translationsLanguage != language && translatedDestination != null && translatedItem.Parent != translatedDestination)
					{
						persister.Move(translatedItem, translatedDestination);
					}
				}
			}
		}

		void persister_ItemSaved(object sender, ItemEventArgs e)
		{
			ContentItem item = e.AffectedItem;
			ILanguage language = gateway.GetLanguageAncestor(item);
			if (language != null)
			{
				int languageKey = item.ID;
				if (context.QueryString[LanguageGateway.LanguageKey] != null)
				{
					int.TryParse(context.QueryString[LanguageGateway.LanguageKey], out languageKey);
				}
				if (item[LanguageGateway.LanguageKey] == null)
				{
					item[LanguageGateway.LanguageKey] = languageKey;
					persister.Save(item);
				}
			}
		}

		public void Stop()
		{
			persister.ItemSaved -= persister_ItemSaved;
			persister.ItemMoved -= persister_ItemMoved;
			persister.ItemDeleted -= persister_ItemDeleted;
		}

		#endregion
	}
}
