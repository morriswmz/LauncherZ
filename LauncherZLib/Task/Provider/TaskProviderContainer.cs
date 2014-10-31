using System;
using System.ComponentModel;
using System.Reflection;
using LauncherZLib.API;

namespace LauncherZLib.Task.Provider
{
    
    internal sealed class TaskProviderContainer
    {

        private ITaskProvider _provider;

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string[] Authors { get; private set; }

        public string Description { get; private set; }

        public static TaskProviderContainer Create(Type providerType)
        {
            if (!(typeof (ITaskProvider).IsAssignableFrom(providerType)))
                return null;
            // read plugin attribute
            var providerAttr =
                Attribute.GetCustomAttribute(providerType, typeof (TaskProviderAttribute)) as TaskProviderAttribute;
            if (providerAttr == null)
                throw new CustomAttributeFormatException("Missing TaskProvider attribute.");
            string[] authors = ParseAuthors(providerAttr.Authors);
            
            // read description attribute
            var descriptionAttr =
                Attribute.GetCustomAttribute(providerType, typeof (DescriptionAttribute)) as DescriptionAttribute;
            var description = descriptionAttr == null ? "" : descriptionAttr.Description;
            
            // read priority attribute
            var priorityAttr =
                Attribute.GetCustomAttribute(providerType, typeof (PriorityAttribute)) as PriorityAttribute;
            var priority = priorityAttr == null ? 0.0 : priorityAttr.Priority;

            // try create

            var container = new TaskProviderContainer
            {
                Id = providerAttr.Id,
                Authors = ParseAuthors(providerAttr.Authors),
                Name = providerAttr.Name
            };
            return container;
        }

        private static string[] ParseAuthors(string authors)
        {
            string[] result = authors.Split(',');
            for (int i = 0; i < result.Length; i++)
                result[i] = result[i].Trim();
            return result;
        }

    }
}
