using System;
using System.IO;
/// <summary>
///  https://stackoverflow.com/questions/3218910/rename-a-file-in-c-sharp
/// </summary>
public static class FileInfoExtensions{
public static void Rename(this FileInfo fileInfo,string fileNewName,FileExistsBehavior fileExistsBehavior=FileExistsBehavior.Rename,string fileExistsRenameComplementsFormat="({0})",params string[]fileExistsRenameComplements){
string fileNewPath=Path.Combine(fileInfo.Directory.FullName,fileNewName);if(File.Exists(fileNewPath)){
switch(fileExistsBehavior){
case FileExistsBehavior.None:
throw new IOException("Can't rename the file "+fileNewPath+" because there's already a file with this name in the destination directory.");
case FileExistsBehavior.Replace:{
File.Delete(fileNewPath);//  Remove the current file so its name can be used
break;}
case FileExistsBehavior.RenameOld:{//  Rename the current file so its old name then can be used.
      string fileOldPath=fileNewPath;//  Save target name that's required; 
                         fileNewPath=validNewName();// and change fileNewPath to a valid new name to use on the current file:
new FileInfo(fileOldPath).Rename(Path.GetFileName(fileNewPath),FileExistsBehavior.Rename);// rename the current file to the new fileNewPath.
                                                  fileNewPath=fileOldPath;//  Finally, use the saved target name for the file to be renamed as the other file is already renamed
break;}
case FileExistsBehavior.Rename:{//  Get another valid name to be used instead and don't touch the existing file
fileNewPath=validNewName();
break;}
case FileExistsBehavior.Skip://  Do nothing...
default:return;
}}
File.Move(fileInfo.FullName,fileNewPath);//  Cloud apps can cause this function to throw a harmless exception after it processes...
string validNewName(){
string fileNewNameWithoutExtension=Path.GetFileNameWithoutExtension(fileNewName),
       fileNewNameExtension       =Path.GetExtension               (fileNewName),
       fileNewNameValidated,
       fileNewPathValidated;
int duplicateCount=0;
 do{duplicateCount++;
    if(fileExistsRenameComplements.Length==0){
       fileNewNameValidated=fileNewNameWithoutExtension+String.Format(fileExistsRenameComplementsFormat,duplicateCount)+fileNewNameExtension;
    }else{
        string[]complements=new string[fileExistsRenameComplements.Length];for(int i=0;i<fileExistsRenameComplements.Length;i++){
                complements[i]=String.IsNullOrEmpty(fileExistsRenameComplements[i])?duplicateCount.ToString():fileExistsRenameComplements[i];
        }
       fileNewNameValidated=fileNewNameWithoutExtension+String.Format(fileExistsRenameComplementsFormat,complements   )+fileNewNameExtension;
    }
                    fileNewPathValidated=Path.Combine(fileInfo.Directory.FullName,fileNewNameValidated);
 }while(File.Exists(fileNewPathValidated));
             return(fileNewPathValidated);
}
}
#region FileExistsBehavior
/// <summary>
///  Behavior if filename already exists.
/// </summary>
public enum FileExistsBehavior{
/// <summary>
///  Skip: skip this file (ignore completely).
/// </summary>
Skip     =0,
/// <summary>
///  Rename: automatically search for a new valid name for the file instead, like the Windows dialog option 'keep both files' behaviour while moving a file.
/// </summary>
Rename   =1,
/// <summary>
///  Same as Rename, but rename the current existing file (the old one), so the new file can get into its place.
/// </summary>
RenameOld=2,
/// <summary>
///  Replace: replace the file in the destination. (Delete the old one.)
/// </summary>
Replace  =3,
/// <summary>
///  None: throw an IOException "Can't rename the file: there's already a file with this name in the destination directory."
/// </summary>
None     =4,
}
#endregion
}