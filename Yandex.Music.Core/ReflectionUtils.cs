using System.Reflection;

namespace Yandex.Music.Core;

internal static class ReflectionUtils
{
    public static void CopyFields(object source, object target, BindingFlags bindingAttr) {
        PropertyInfo[] sourceProps = source.GetType().GetProperties(bindingAttr);
        foreach (PropertyInfo sourceProp in sourceProps) {
            PropertyInfo targetProp = target.GetType().GetProperty(sourceProp.Name, bindingAttr);
            if (targetProp != null && targetProp.CanWrite) {
                object sourcePropertyValue = sourceProp.GetValue(source, null);
                targetProp.SetValue(target, sourcePropertyValue, null);
            }
        }
    }

    public static void CopyPublicFields(object source, object target) {
        CopyFields(source, target, BindingFlags.Instance | BindingFlags.Public);
    }
}
