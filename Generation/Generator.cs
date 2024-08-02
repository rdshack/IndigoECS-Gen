namespace ecs_gen.Generation;

public static class Generator
{
  public static void Gen(string schemaPath, string outputPath)
  {
    SchemaParser parser = new SchemaParser();
    var parseResult = parser.Parse(schemaPath);
    
    System.IO.DirectoryInfo di = new DirectoryInfo(outputPath);
    foreach (FileInfo file in di.EnumerateFiles())
    {
      file.Delete(); 
    }
    foreach (DirectoryInfo dir in di.EnumerateDirectories())
    {
      dir.Delete(true); 
    }
    
    ComponentGen.Gen(outputPath, parseResult);
    ComponentCopierGen.Gen(outputPath, parseResult);
    FlatBuffersGen.Gen(outputPath, parseResult);
    FrameSerializerGen.Gen(outputPath, parseResult);
    ComponentTypeIndexGen.Gen(outputPath, parseResult);
    AliasLookupGen.Gen(outputPath, parseResult);
    EnumGen.Gen(outputPath, parseResult);
  }
}