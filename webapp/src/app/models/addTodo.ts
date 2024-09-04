export interface AddTodo {
    Title: string;
    Notes: string;
    DueOn: Date;
    File: FileData | null;
    Email: string;
    Token: number;
}


export interface FileData {
    FileB64: string | ArrayBuffer | null;
    FileName: string;
    FileSize: number;
    FileType: string;
}