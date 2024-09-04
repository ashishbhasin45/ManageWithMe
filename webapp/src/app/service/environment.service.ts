import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class EnvironmentService {
    private env: any;

    constructor() {
        this.env = (window as any).env || {};
    }

    get apiUrl(): string {
        return this.env.apiUrl || 'https://6j5f2lq494.execute-api.us-east-1.amazonaws.com/prod/';
    }
}