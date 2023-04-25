using FlatRedBall.Glue.MVVM;

namespace WfcPlugin.ViewModels
{
    public class WfcEditorViewModel : PropertyListContainerViewModel
    {
        [SyncedProperty]
        public double Speed
        {
            get => Get<double>();
            set => SetAndPersist(value);
        }

        [SyncedProperty]
        public int? Seed
        {
            get => Get<int?>();
            set => SetAndPersist(value);
        }

        public override void UpdateFromGlueObject()
        {
            base.UpdateFromGlueObject();

            SetDefault(nameof(Speed), 64d);
        }

        private void SetDefault<T>(string name, T value)
        {
            if (GlueObject == null)
            {
                return;
            }

            var property = GlueObject.Properties.Find(i => i.Name == nameof(Speed));
            if (property != null)
            {
                return;
            }

            SetAndPersist(value, name);
        }
    }
}
