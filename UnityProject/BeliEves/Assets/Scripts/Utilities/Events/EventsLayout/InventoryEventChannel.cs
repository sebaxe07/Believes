namespace Utilities.Events.EventsLayout {
    public class InventoryEventChannel : EventBodySwitch {
        public float savedStamina;
        public float savedHealth;

        public void UpdateValue(BodyType bodyType, float stamina, float health) {
            this.savedStamina = stamina;
            this.savedHealth = health;
            base.UpdateValue(bodyType);
        }

    }
}