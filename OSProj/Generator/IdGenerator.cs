namespace OSProj.Generator
{
  public class IdGenerator
  {
    private static int _count = 0;
    public static int Id { get { return ++_count; } }

  }
}