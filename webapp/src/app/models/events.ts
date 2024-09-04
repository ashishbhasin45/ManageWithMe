export interface events {
    Date: Date;
    Todos: todo[];
}

export interface todo {
    Title: string;
    Notes: string;
    DueOn: Date;
    IsCompleted: boolean;
    TodoId: string;
    HasFiles: boolean;
    softComplete: boolean;
}