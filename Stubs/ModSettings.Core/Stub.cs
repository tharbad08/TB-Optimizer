using System;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
namespace ModSettings.Core
{
 [Flags] public enum ModSettingsContext { None=0, MainMenu=1, Game=2, MapEditor=4, All=7 }
 public sealed class ModSettingDescriptor
 {
    public static ModSettingDescriptor Create(string name)=>new ModSettingDescriptor();
    public ModSettingDescriptor SetTooltip(string tooltip)=>this;
    public ModSettingDescriptor SetEnableCondition(Func<bool> predicate)=>this;
 }
 public abstract class ModSetting
 {
    protected ModSetting(ModSettingDescriptor descriptor) { }
    public abstract void Reset();
 }
 public class ModSetting<T> : ModSetting
 {
    public ModSetting(T defaultValue, ModSettingDescriptor descriptor):base(descriptor) { DefaultValue=defaultValue; }
    public T DefaultValue { get; }
    public T Value { get; private set; }=default!;
    public event EventHandler<T>? ValueChanged;
    public virtual void SetValue(T value){Value=value;ValueChanged?.Invoke(this,value);}
    public override void Reset()=>SetValue(DefaultValue);
 }
 public abstract class NonPersistentSetting : ModSetting { protected NonPersistentSetting(ModSettingDescriptor descriptor):base(descriptor){} }
 public sealed class ModSettingsOwnerRegistry { }
 public abstract class ModSettingsOwner
 {
    protected ModSettingsOwner(ISettings settings,ModSettingsOwnerRegistry registry,ModRepository repository){}
    public abstract string ModId {get;}
    public virtual ModSettingsContext ChangeableOn=>ModSettingsContext.MainMenu;
    protected virtual void OnBeforeLoad(){}
    public void AddNonPersistentModSetting(NonPersistentSetting setting){}
    public void AddCustomModSetting<T>(ModSetting<T> setting,string id){}
 }
}