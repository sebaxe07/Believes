using Events.EventsLayout;


namespace Utilities.Events.EventsLayout
{
    public enum BodyType
    {
        Eve,
        Robot,
        Tank,
        Agile,
        Support
    }

    public class EventBodySwitch : BasicEventChannel {
        public BodyType bodyType = BodyType.Eve;
        
        public void UpdateValue(BodyType bodyType){
            this.bodyType = bodyType;
            RaiseEvent();
        }
    }

}