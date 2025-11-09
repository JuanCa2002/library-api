using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace LibraryAPI.Utilities
{
    public class GroupByVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var namespaceController = controller.ControllerType.Namespace;
            var version = namespaceController!.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}
