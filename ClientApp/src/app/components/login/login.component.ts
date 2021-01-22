import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { SessionService } from '../../services/session.service';
import { NgForm } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(private authService: AuthService, private sessionHelper: SessionService, private httpClient: HttpClient) { }

  ngOnInit() {
  }

  /**
   * Sends credentials from form in an authentication attempt. Success results in a session being created.
   * @param form
   */
  login(form: NgForm) {

    this.authService.login().subscribe(resp => {
       const redirect = resp;
       window.open(redirect);
      /* this.httpClient.get(redirect, {"responseType": "text"})
       .subscribe(x => {  return x; }, err => { console.error(2, err); } );
        this.sessionHelper.startSession(<string>resp);*/
      }, err => { console.error(err); });
  }



}
