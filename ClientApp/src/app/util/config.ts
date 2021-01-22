import { Injectable } from '@angular/core';
import {environment} from './../../environments/environment';
import { HttpHeaders } from '@angular/common/http';

@Injectable()
export class Configuration {


  public readonly JWT: string = "web-token";
  REDIRECT: string;
  private readonly _version: string;
  private readonly _headers: {};

  constructor() {
    this._version = "1.0.0";
  }

  get version(): string { return this._version; }

  /**
   * Returned value contains an authorization header
   */
  get headers(): object { return this._headers; }

   getToken(): string {
    return localStorage.getItem(this.JWT);
  }

}
