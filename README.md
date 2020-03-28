# SelectFile

SelectFile allows you to select files and directories grapically for use in batch files.

## How to use

SelectFile is a command line utility.
Without any arguments, it shows a file selection window with all file types visible.
The selected file(s)/directory will be written to the console for further processing.
You can process them in a loop as shown below:

    REM Replace %I with %%I if you do this in a batch file
    FOR /F %I IN ('SelectFile.exe /multi /mask txt') DO (
	REM Full file path is in %I
	REM Do something with the file here
	)

Please note that `SelectFile.exe | SET /P FILENAME=` will not work due to how variables are handled.
It's not possible in Windows to set variables on the parent process without some nasty DLL injection attack.
If you want to avoid loops, this will work for a single file:

    SelectFile.exe /mask txt>file.temp
	SET /P FILE=<file.temp
	DEL file.temp
	IF "%FILE%"=="" GOTO NOFILE
	REM Do something with the FILE variable here

## Command line arguments

The command line arguments differ between file and directory selection.
Be sure to consult the correct section.
Arguments that take a parameter are also given an example.

All arguments are optional and the order doesn't matters.

### File selection

The arguments below apply to the file selection dialog

#### /title

Defines the title of the window.
If not specified, the title of the console window is used,
this is usually the current command that's being run.

Example: `/title "Please select a text file"`

#### /start

Directory to start browsing in.
This is the directory that the file dialog will be initially showing.
The user can still freely select any parent directory or other drive.

Example: `/start "C:\Log Files"`

#### /save

Show the "save file" dialog instead of the open dialog.

Differences:

- Dialog will allow the user to continue if the target file doesn't exists
- Dialog shows an overwrite warning to the user if an existing file is selected

The dialog will not touch any file, so even if the user confirms the overwrite prompt,
the file will not be truncated.

This argument can't be used together with `/multi`

#### /multi

Allow multiple file selections by holding the CTRL or SHIFT key.

This argument can't be used together with `/save`

#### /default

This is the file name that will be initially filled into the file name field.
It does not needs to match any of the defined masks but obviously should.

Example: `/default "proposed File.txt"`

#### /mask

- Format 1: `/mask name=value`
- Format 2: `/mask ext`

This argument specifies the file name masks available to the user.
The argument is repeatable to specify multiple masks and both formats can be used together.
The first mask argument is the one in use by default in the dialog.
A mask can contain multiple extensions.
If the argument is never specified, `/mask "All files (*.*)=*.*"` will be used automatically.
If the mask is in the first format, the name will be shown as a selection in the window,
and the value will be the filtered extensions.
This format allows you to specify multiple extensions at once using semicolons (see first example below)

If the second format is used, the name part is read from the registry.
If the selected name can't be found in the registry, it will be substituted using a generic display string:
`/mask "*.ext files=*.ext"`

Notes:

- The 'All files' type is not present by default if you specify your own masks. You can add it again by specifying it.
- Users can always override your selection by typing `*.*` as name. This is a feature of Windows and not this application.
- It's common but not required that you show the file masks in the name.
- You can use masks to limit users to a single file name

Examples:

    /mask "Text files=*.txt;*.ini;*.log"

Shows "Text files" as file type. The dialog shows all files with a "txt", "ini" or "log" extension.

	/mask "All files (*.*)=*.*"

This is the same as specifying no mask.

	/mask doc

This will likely show up as "Word Document" in the filter, provided an application has registered the doc type.
The text depends on your installed software as well as the local language.

    /mask "MyApp configuration=custom_name.config"

This filters for a single file name

### Directory selection

These arguments are valid for the directory selection window

#### /dir

Enable directory mode.
Without this argument, a file selection dialog is shown instead (see "File selection" above)

#### /title

Defines the title of the window

Example: `/title "Please select the log file directory"`

#### /new

Shows a "New Directory" button

#### /start

Directory to initially select.
A user can still select a completely different directory,
including one on a different drive.

Example: `/start C:\Log Files`
