namespace Sitecore.Rocks.Plugin.Commands.DistributedDb
{
    using Sitecore.Rocks.Plugin.DistributedDb.Tools;
    using Sitecore.VisualStudio.Commands;

    [Command(Submenu = "Distributed Databases")]
    public class SynchronizeCommand : CommandBase
    {
        public SynchronizeCommand()
        {
            this.Text = "Synchronize";
            this.Group = "Distributed Databases";
            this.SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            return CommandWrapper.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            CommandWrapper.Execute(parameter, "Synchronize");
        }
    }
}