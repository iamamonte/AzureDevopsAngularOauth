import { Injectable } from '@angular/core';
import { Configuration } from '../util/config';
import { Router } from '@angular/router';
import { Local } from 'protractor/built/driverProviders';


@Injectable()
export class SessionService {

  private _loggedIn = false;

  constructor(private config: Configuration, private router: Router) { }

  startSession(jwt: string) {
    // TODO: log
    localStorage.setItem(this.config.JWT, jwt);

    const redirect = localStorage.getItem(this.config.REDIRECT);
    if (redirect != null) {
      localStorage.removeItem(this.config.REDIRECT);
      this.router.navigateByUrl(redirect);
    }
    this._loggedIn = true;

  }

  get loggedIn(): boolean {
    return this._loggedIn;
  }
}
