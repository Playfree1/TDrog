namespace TowerDefecse
{
    public class Resources
    {
        
       public enum Resource
        {
            wood = 0,
            rock =1,
            iron = 2,
        }
        private static readonly Dictionary<Resource, int> _resources = new();
        public static void ChangeResources(Resource type, int count)
        {
            _resources[type] = _resources.GetValueOrDefault(type) + count;
        }
        public static int GetResource(Resource type) => _resources.GetValueOrDefault(type);
        
    }
}